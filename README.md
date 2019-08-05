# StudyLog
+ 学习记录，将记录学习中产生的疑问、工作中出现的Bug以及相关的解决方案（如果有）
+ 问题将被记录在`Question`目录
+ 观点、思想将被记录在`Standpoint`目录

# Plan
+ 信息管理demo  
  + mysql/sqlserver
    - [ ] c/s  
  + layUI  
    - [ ] b/s
  + 根据手机号判断是否是IOS用户
    - [ ] IOS接口开发
+ Common
  + - [x] HttpHelper  支持async/await，基于HttpClient
  + - [x] AttibuteHelper 支持二次封装
  + - [x] FileHelper 支持文件、目录 add/delete/move/copy，支持压缩/解压(zip)
  + - [x] MailHelper 支持async/await，基于SmtpClient
+ MonitorApp
  + - [x] 进程监控Dome
+ NLogApp
  + - [x] NLog 简单使用与简单配置
+ AppDomainTestApp
  + - [x] 跨AppDomain通信简单学习
+ WebSocketClientApp
  + - [x] WsServer调试工具。Win10与Win10以下需要区分使用。原因是Win10以下没有实现Websocket协议
+ 自动化部署
  + 开发机器
  + 生产服务器 _一台 CentOS 7 虚拟机_
    + - [x] Docker 环境
      + - [ ] 接收远程 Docker 命令
  + Build Hub 服务器 _一台 CentOS 7 虚拟机_
    + - [x] Git 环境
    + - [ ] Git web hook 服务。仅 Develop 分枝
    + - [x] Docker 环境
    + - [x] .Net Core 环境
    + - [ ] 自动化部署
      + - [ ] 主控程序
  + 主控程序 _Master_ _远程管理服务程序_
    + 管理服务程序
    + 创建Docker Image
  + 服务程序 _App_
    + 服务状态调整
    + 程序信息整理
    + 一些其他功能
  + Git
    + - [ ] 设置 Web hook 服务地址
    + - [x] Master 生产分枝
    + - [ ] ~~Release 测试分枝~~
    + - [x] Develop 开发分枝
  + Docker
    + Build 服务器
      - [ ] Build Images
      - [ ] Tag Images
      - [ ] Push Images
    + 生产/测试环境
      - [ ] Pull Images
      - [ ] Kill Content
      - [ ] Rm Content
      - [ ] Run Content
  + 流程规划
    1. 提交代码
    2. Publish Core
      + Git Web Hook 推送代码到 Build 服务器
      + Build 服务器拉取代码
    3. Docker Build  Images
    4. Docker Push Images
    5. Docker Pull Images
    6. Restart Container
 + 主控程序实现 _接口名命名规则从使用者角度出发。如果有疑惑则从使用者角度理解_
  + BLL 层内容
  + 全局信息
    + Docker Image 版本信息
    + 生产服务器IP信息
  + Ws 接口 _App 使用_
    + - [ ] SendAppInfo
        + 实现功能：接收 App 信息，根据 App 信息管理 Docker 更新
        + 实现流程：
          + - [ ] 当 App 信息小于当前版本号则调用 `Offline` 接口
          + - [ ] 下线状态的 App 等待任务数为零执行 `Docker Deploy` 命令
  + HTTP 接口 _Get Hook_
    + - [ ] DevelopHook
      + - [ ] 处理 Develop 分枝更新
      + - [ ] 执行 Docker Build 文件
  + DAL 层内容
    + Docker Build _本地命令文件_
      + - [ ] git pull develop
      + - [ ] dotnet publish -c Release
      + - [ ] docker Build name:tag /xxx/publish
        + - [ ] Dockerfile
      + - [ ] docker Tag hubname:tag name
      + - [ ] docker push hubname
    + Docker Deploy _远程命令文件_
      + - [ ] Pull Images
      + - [ ] Kill Content
      + - [ ] Rm Content
          + - [ ] Run Content  
    + App 信息
      + IP
      + OnlineState
      + TaskCount
      + Version
  + 服务程序实现 _接口名命名规则从使用者角度出发。如果有疑惑则从使用者角度理解_
    + BLL 层内容
      + - [ ] 服务状态跳转
        + - [ ] 上线状态接口。开启接收任务
        + - [ ] 下线状态接口。停止接收任务
      + - [ ] 程序信息整理
        + - [ ] 推送 App 信息到中控
    + DAL 层内容
      + App 信息
        + IP
        + OnlineState
        + TaskCount
        + Version
      + - [ ] 一些其他功能
    
