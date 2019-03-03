# SDKService
+ 向SDK下发TaskToken
+ 接收SDK回发TaskDefinition
+ 执行Crawl
+ 记录TaskStatus

---

|模块|依赖|Status|
|:---:|:---:|:---:|
|MVC|Crawl|30%搁浅
|Crawl|MYSQL|20%
|MYSQL||100%|

---

## Crawl依赖
|Crawl|Status|
|:---:|:---:|
|Http|50%
|Porxy|80%
|TaskLog|50%
|TaskLogLevel|100%
|HttpLog|80%
|LogFile|100%

---

## Log
|Log|Status|
|:---:|---|
|TaskLog|80%
|HttpLog|80%
|DataLog|80%
|ILog|100%

---

## Crawl模块
|Crawl|Status
|:---:|:---:|
|属性|100%(ADO模型)
|TaskLog|100%
|HttpLog|100%
|DataLog|100%
|Start()|80%
|Finish()|50%

|CrawlType|Status
|:---:|:---:|
|zhima_souce|
|taobaoH5|80%


### 流程控制
|场景|处理|
|---|---|
|函数能够在多个节点抛出可重试异常|捕获异常使其不影响抓取流程
|抛出结束性异常|结束抓取流程
|抛出未知性异常|结束抓取流程并通知
|抛出IO异常|记录本地日志

|状态|Class|备注
|:---:|---|---|
|可重试异常|HTTPTimeoutException<br />HttpRequestException
|结束性异常|CollectFailedException
|未知异常|Exception|所有异常，应该记录日志

抓取实现类只实现抓取
DB操作放在基类中完成

---

## 业务流程
|顺序|C|S|备注
|---|---|---|---|
|获取Token|请求S获取Token<br />提交Phone、company、CrawlType|生成Token保存数据库
|执行登录|执行者。回写日志|
|登录完成|根据TaskDefinition发送登录结果到S|验证Token存在、TaskDefinition可用性
|执行抓取||执行抓取。记录日志
|完成||保存日志|



 
-------------------------------
|新+流程|详细|目的|
|---|---|---|
|添加C向S发送DeveiceInfo|
|细化S向C下发详细字段：{cookie:[xxxxxx],property:[xxxxx],BeginUrl:[xxxx],FinishUrl:[xxxx]}|添加S向C下发beginUrl，FinishUrl 针对H5Crawl<br/>SDKCrawl只下发参数名，不指定参数出现地点|为了取消H5在WebView中的跳转操作，增加用户体验

---
## 逻辑优化
|问题|改动|
|---|---|
|ServiceAggregation类不应关注ServerCallBack信息|ServerCallBack信息改动到HTTPListenHelper中。ServiceAggregation只需要返回执行结果
|




-----

# 2019年3月3日 18:27:01
## 开始sdkService优化
|流程|实现|目的|时间|
|---|---|---|---|
|优化HTTP模块|0%|将HTTP于ORDER类解耦<br />CRAWLER将与HTTP耦合
|优化PROXY模块|0%|HTTPPROXY借还
|优化爬虫模块|0%|将现有爬虫使用新的CRAWLER框架重写
|优化日志模块|0%|记录日志
|SDK接口模块|50%|目前使用WINFORM+HTTPLISTEN实现，在上方工作完成后将其改为MVC网站