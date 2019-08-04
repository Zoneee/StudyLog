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
  + 主控程序 _远程管理服务程序_
    + - [ ] 管理服务程序
      + - [ ] 上/下线
    + - [ ] 管理生产服务器Docker更新
      + - [ ] docker stack
    + - [ ] 接收 Git Event
      + 本地命令
        + - [ ] 处理 Develop 分枝更新事件
        + - [ ] git pull develop
        + - [ ] dotnet publish -c Release
        + - [ ] docker Build name:tag /xxx/publish
          + - [ ] Dockerfile
        + - [ ] docker push name
      + 远程命令
        + - [ ] Pull Images
        + - [ ] Kill Content
        + - [ ] Rm Content
        + - [ ] Run Content  
  + 服务程序
    + - [ ] 开监听。开启接收任务
    + - [ ] 关监听。停止接收任务
    + - [ ] 任务数推送。推送当前任务数到主控
    + - [ ] 一些其他功能
  + Git
    + - [ ] 设置 Web hook 服务地址
    + - [ ] Master 生产分枝
    + - [ ] ~~Release 测试分枝~~
    + - [ ] Develop 开发分枝
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
  + 流程
    1. 提交代码
    2. Publish Core
      + Git Web Hook 推送代码到 Build 服务器
      + Build 主动拉取代码 Build
    3. Build Docker Images
    4. Push Docker Images
    5. Pull Docker Images
    6. Build Content And Restart
