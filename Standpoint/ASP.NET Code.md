# Startup
## `ConfigureService`与`Configure`
+ 在`ConfigureService`函数中配置服务。
  + 服务可以是提供功能的可复用组件。
  + 添加服务配置也称为**注册**。通过依存关系注入。
  + 添加的服务可以在整个`ApplicationService`中使用。
  + 在`Configure`前执行
+ 在`Configure`函数中配置**请求处理管道**
+ 在应用启动时，ASP.NET Core 运行时会调用 `ConfigureServices` 和 `Configure`
## 使用与配置
+ 按照命名约定
+ 通常通过在`Program.Main`调用 `WebHostBuilderExtensions.UseStartup<TStartup>` 来指定 `Startup` 类
+ 可以不使用`Startup`多次调用`IWebHostBuilder.ConfigureAppConfiguration`为`ApplicationService`添加配置
+ 使用筛选器扩展配置
  + 通过实现`IStartupFilter`进行扩展
  1. 在`ClassA`实现扩展功能
  2. 在`ClassB:IStartupFilter`调用`UseMiddleware<T>()`注册`ClassA`
  3. 在`ConfigureService`中注册`ClassB`。使用方法见：**使用与配置**

# 依赖关系注入
## 依赖关系与注入服务
+ 使用接口或基类抽象化依赖关系实现。
+ 注册服务容器中的依赖关系。 使用内置的服务容器 `IServiceProvider`。 服务已在应用的 `Startup.ConfigureServices` 方法中注册。
+ 将服务注入 到使用它的类的构造函数中。 框架负责创建依赖关系的实例，并在不再需要时对其进行处理。
+ 例：
  + 在`Startup.ConfigureServices`注册
      ```C
      services.AddScoped<interface, T>();  //通过接口或抽象类
      services.AddSingleton(typeof(ILogger<T>), typeof(Logger<T>));  //通过泛型
      ```
  + 通过构造函数注册
      ```C
      class A { /*...*/ }
      class B{
          private readonly A _a;
          public B(A a){
              _a=a;
          }
      }
      ```
## 服务生命周期、注册设计与上下文服务访问
+ 生命周期
  + **暂时**。每次请求时创建
  + **范围内**。每个客户端连接时创建（使用长连接的客户端只创建一次）
    + `在中间件内使用有作用域的服务时，请将该服务注入至 Invoke 或 InvokeAsync 方法。 请不要通过构造函数注入进行注入，因为它会强制服务的行为与单一实例类似`
  + **单例**。整个`Application`第一次被请求时创建
    + `从单一实例解析有作用域的服务很危险。 当处理后续请求时，它可能会导致服务处于不正确的状态。`
+ 作用域验证
  + 创建非**单例**服务时应注意：
    + 服务不能直接或间接被**根**解析(访问)到
    + 服务不能直接或间接的注册到**单例**
  + 在根容器创建的的作用域的生命周期，将会被提升为**单例**。因为根容器只在`ApplicationService`被关闭时施放。
+ 注册设计原则
  + 设计服务以使用依赖关系注入来获取其依赖关系。
    + 不必拘泥于在`Startup中service.Add`或通过构造函数注册的方式选择上，只要保证合理的依赖关系即可。
  + 避免进行有状态的静态方法调用。
  + 避免在服务中直接实例化依赖类。 直接实例化将代码耦合到特定实现。
  + 不在应用类中包含过多内容，确保设计规范，并易于测试。
  + `Application`不会为实现`IDisposable`的服务自动调用`Dispose`
+ 注册方法
  + [官方文档](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.2#service-registration-methods)
  + 使用`Try...`函数注册时，应用会检查接口是否有相同类型注册，在有同类型注册时后注册的不生效
    ```C
    services.AddSingleton<IMyDependency, MyDependency>();
    // 因为 IMyDependency 被 MyDependency 注册。故 DifferentDependency 不生效
    services.TryAddSingleton<IMyDependency, DifferentDependency>();
    ```
+ 访问服务
  + 注册的服务可以通过`HttpContext.RequestServices`集合访问
  + 通常不推荐通过`HttpContext.RequestServices`访问。建议通过构造函数注册服务，在对应类访问对应服务

+ 线程安全
  + 创建线程安全的单一实例服务。 如果单例服务依赖于一个瞬时服务，那么瞬时服务可能也需要线程安全，具体取决于单例使用它的方式。
  + 单个服务的工厂方法（例如 `AddSingleton<TService>(IServiceCollection, Func<IServiceProvider,TService>`) 的第二个参数）不必是线程安全的。 像`static` 构造函数一样，它保证由单个线程调用一次

# 中间件
## 定义
+ 请求委托用于生成请求管道。 请求委托处理每个 HTTP 请求。
  + 选择是否将请求传递到管道中的下一个组件。
  + 可在管道中的下一个组件前后执行工作。
+ 可将一个单独的请求委托并行指定为匿名方法（称为**并行中间件**），或在可重用的类中对其进行定义。 这些可重用的类和并行匿名方法即为中间件 ，也叫中间件组件 。 请求管道中的每个中间件组件负责调用管道中的下一个组件，或使管道短路。 当**中间件短路时，它被称为“终端中间件” ，因为它阻止中间件进一步处理请求**。
## 配置中间件
+ `RunMap`
  + 在结尾使用。是一种约定。并且某些中间件组件可公开在管道末尾运行的 `Run[Middleware]` 方法
+ `Use`
  + 将多个请求委托链接在一起。 `next` 参数表示管道中的下一个委托。 可通过不 调用 `next` 参数使管道**短路**。 通常可在下一个委托前后执行操作
+ `Map/MapWhen`
  + 用作管理请求管道分支。支持嵌套与多段匹配
  ```C
    //将捕获 http://host/map1 请求
    app.Map("/map1", HandleMapTest1);
    //将捕获 http://host/map1/seg1 请求。多段匹配
    app.Map("/map1/seg1", HandleMultiSeg);
    //将捕获 满足谓词条件 请求
    app.MapWhen(context => context.Request.Query.ContainsKey("branch"),HandleBranch);
  ```
## 执行顺序
+ 中间件的添加顺序决定了执行顺序
+ [内置中间件文档](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/middleware/?view=aspnetcore-2.2#built-in-middleware)
+ 常见应用方案：
  1. 异常/错误处理
     + 当应用在开发环境中运行时：
       + 开发人员异常页中间件 (`UseDeveloperExceptionPage`) 报告应用运行时错误。
       + 数据库错误页中间件 (`UseDatabaseErrorPage`) 报告数据库运行时错误。
     + 当应用在生产环境中运行时：
       + 异常处理程序中间件 (`UseExceptionHandler`) 捕获以下中间件中引发的异常。
       + `HTTP` 严格传输安全协议 (`HSTS`) 中间件 (`UseHsts`) 添加 `Strict-Transport-Security` 标头。
  2. `HTTPS` 重定向中间件 (`UseHttpsRedirection`) 。将 `HTTP` 请求重定向到 `HTTPS。`
  3. 静态文件中间件 (`UseStaticFiles`) 。返回静态文件，并简化进一步请求处理。
  4. `Cookie` 策略中间件 (`UseCookiePolicy`) 。使应用符合欧盟一般数据保护条例 (GDPR) 规定。
  5. 身份验证中间件 (`UseAuthentication`) **未经身份验证的请求不会立即短路。 虽然身份验证中间件对请求进行身份验证，但仅在 `MVC` 选择特定 `Razor` 页或 `MVC` 控制器和操作后，才发生授权（和拒绝）。**
  6. 会话中间件 (`UseSession`) 。建立和维护会话状态。 如果应用使用会话状态，请在 `Cookie` 策略中间件之后和 `MVC` 中间件之前调用会话中间件。
  7. `MVC` (`UseMvc`) 。将 `MVC` 添加到请求管道。

# Host
