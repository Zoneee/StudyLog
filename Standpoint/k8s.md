# 简介
**简单说下`k8s`就是`kubernetes`简写。除去开头结尾刚好8个字母，所以叫`k8s`**  
`k8s` 用于管理容器可以是 `Docker` 或什么其他   
`k8s` 似乎分为 `master`与`client`。
两者都是寄宿在操作系统中。
通过`CLI`调用`API`，远程需要借助`SSH`。  
_可以将`CLI`写在`YAML`文件中，分发到每个`client`中，通过`YAML`控制_  
_需要设置 `Volume`、`Port`以及`image`版本固定`laster`_


# 作用
实现 `Docker` 管理。热部署

# 笔记
## [k8s部署教程](https://kubernetes.io/docs/tasks/run-application/run-stateless-application-deployment/)  
YAML 文件字段记录
+ `kind` ：资源类型。Pod、Deployment、Service等
+ `metadata` ：App 描述信息。name、namespace、tag等
+ `spe` ：对`Pod`对象定义。指定 `container(容器)、storage、volume(磁盘卷)` 等参数信息。可以用 `list` 描述
  + name
  + image
  + command
  + args
  + workingDir
  + ports
  + env
  + resource
  + volumeMounts
  + livenessProbe
  + readinessProbe
  + livecycle
  + terminationMessagePath
  + imagePullPolicy
  + securityContext
  + stdin
  + stdinOnce
  + tty
