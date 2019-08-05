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
