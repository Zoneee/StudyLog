# 简介
> **简单说下`k8s`就是`kubernetes`简写。除去开头结尾刚好8个字母，所以叫`k8s`**  

`k8s` 由多个模块组合执行。见[模块](#%e6%a8%a1%e5%9d%97)章节  
`k8s` 用于管理容器。可以是 `Docker` 或什么其他   
`k8s` 分为 `master` 与 `client`。默认 `master` 与 `client` 是独立的机器  
`k8s` 与 `Docker` 两者都是寄宿在操作系统中。通过`CLI`调用`API`。远程需要借助`SSH`。  
_可以将`CLI`写在`YAML`文件中，分发到每个`client`中，通过`YAML`控制_  
_需要在 `YAML` 中设置 `Volume`、`Port`以及`image`版本固定`laster`_

# 文档&教程
> _官方文档停留在v1.15.2，实际版本在v2.0.0。鉴于国内通过镜像只能获取到v1.15.2版本，所以没差_  

[官方文档](https://kubernetes.io/)   
[官方k8s部署教程](https://kubernetes.io/docs/tasks/run-application/run-stateless-application-deployment/)  
[**通过配置文件方式初始化`kubeadm`** 教程大纲](https://www.cnblogs.com/RainingNight/p/using-kubeadm-to-create-a-cluster-1-12.html)  
[**通过修改`docker`镜像`tag`方式初始化`kubeadm`** 教程大纲](https://www.cnblogs.com/RainingNight/p/using-kubeadm-to-create-a-cluster.html)  
[**处理`docker`容器运行异常**参考文章](https://blog.csdn.net/sanpic/article/details/87084779)  
[**`Calico` 安装**教程大纲](https://docs.projectcalico.org/v3.8/getting-started/kubernetes/installation/calico)

# 作用
实现 `Docker` 管理。热部署

# 模块
工具|简介|类型
---|---|---
kubectl|一个命令行界面，用于运行针对Kubernetes集群的命令|_**CLI**_
kubelet|在每个节点上运行的主要“节点代理”。主要用于管理Pod|_**命令行工具**_
kubeadm|旨在提供kubeadm init和kubeadm join最佳实践“快速路径”，用于创建Kubernetes集群|_**设置工具**_
minikube|对k8s封装。本地k8s工具|_**设置工具**(学习用)_

> 设置工具基于命令行工具，命令行工具基于CLI  
> 其中 minikube 不建议在生产使用  
> minikube 官网描述：  
> 
> ```
> Why do I want it?  
>     If you would like to develop Kubernetes applications:  
>         locally
>         offline
>         using the latest version of Kubernetes
>     Then minikube is for you.  
> What is it good for? Developing local Kubernetes applications
> What is it not good for? Production Kubernetes deployments
> What is it not yet good for? Environments which do not allow VM’s
> ```

# 笔记
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

# 环境配置
+ OS：CentOS-7
1. 修改网络环境
    ```
    vi /etc/sysconfig/network-scripts/ifcfg-eth0 
    ```
2. 安装环境 _国内可能无法直连k8s，见`设置阿里云镜像`_
    ```
    yum install -y kubelet kubeadm kubectl docker
    ```
3. 为 `yum` 设置阿里云镜像
    ```
    cat <<EOF > /etc/yum.repos.d/kubernetes.repo
    [kubernetes]
    name=Kubernetes
    baseurl=https://mirrors.aliyun.com/kubernetes/yum/repos/kubernetes-el7-x86_64/
    enabled=1
    gpgcheck=1
    repo_gpgcheck=1
    gpgkey=https://mirrors.aliyun.com/kubernetes/yum/doc/yum-key.gpg https://mirrors.aliyun.com/kubernetes/yum/doc/rpm-package-key.gpg
    EOF
    ```
1. 禁用 swap
    ```
    sudo swapoff -a
    ```
1. 关闭防火墙
    ```
    systemctl stop firewalld.service _关闭_
    systemctl disable firewalld.service _禁用自启_
    ```
1. k8s初始化 _`kubeadm config images pull` 失败见 [国内获取k8s image](#%e5%9b%bd%e5%86%85%e8%8e%b7%e5%8f%96k8s-image)_
    ```
    kubeadm init
    ```
1. 安装网络组件 [Calico](https://docs.projectcalico.org/v3.8/getting-started/kubernetes/installation/calico)
   + _用于 `pod` 间通讯。你可能用不上，但 `k8s` 用得上，不装不行_
    ```
    curl https://docs.projectcalico.org/v3.8/manifests/calico.yaml -O
    kubectl apply -f calico.yaml
    ```
1. 
> 注：
> + 重置+启动+初始化
    ```
    kubeadm reset && systemctl start  kubelet && kubeadm init 
    ```
> + 屏蔽Error级错误
    ```
    kubeadm reset && systemctl start  kubelet && kubeadm init --ignore-preflight-errors=all
    ```

# 国内获取k8s image
## 方法一：修改配置文件
> 适用于 `k8s v1.11+` 版本 
> [**教程大纲**](https://www.cnblogs.com/RainingNight/p/using-kubeadm-to-create-a-cluster-1-12.html)  

1. 输出`kubeadm`到`kubeadm.conf`文件
    ```
    kubeadm config print init-defaults > kubeadm.conf
    ```
2. 修改`image repository`地址
   + [使用 gcr.azk8s.cn 镜像](http://mirror.azure.cn/help/gcr-proxy-cache.html)
    ```
    vim kubeadm.conf
    imageRepository 字段内容修改为 gcr.azk8s.cn 
    ```
3. `pull` 镜像
   ```
   kubeadm config images pull --config kubeadm.conf
   ```
    > `v1.15.2` 版本可以使用 `kubeadm init --config` 替换命令
   

## 方法二：docker+tag
> tag 需要根据kubeadm版本改动，具体版本可以在配置文件中查询输出，详见 [输出配置文件](#%e6%96%b9%e6%b3%95%e4%b8%80%e4%bf%ae%e6%94%b9%e9%85%8d%e7%bd%ae%e6%96%87%e4%bb%b6)  
> 该方法细节过多，操作繁琐，不建议使用。只给出[**教程大纲**](https://www.cnblogs.com/RainingNight/p/using-kubeadm-to-create-a-cluster.html)

# `kubeadm init` 异常处理
+ 确保 `master` 节点可访问 _`advertiseAddress` 字段_。[导出配置文件](#%e6%96%b9%e6%b3%95%e4%b8%80%e4%bf%ae%e6%94%b9%e9%85%8d%e7%bd%ae%e6%96%87%e4%bb%b6)中默认的 `master ip` 为 `1.2.3.4` 需要手动修改。 
+ 确保 `kubelet` 版本匹配 _`kubernetesVersion` 字段_
+ 确保 `docker`、`kubelet` 服务正确运行
+ 确保 `docker` 容器正常运行。[参考文章](https://blog.csdn.net/sanpic/article/details/87084779)

# 其他异常处理
+ `The connection to the server localhost:8080 was refused - did you specify the right host or port?`
    ```
    sudo mkdir ~/.kube
    sudo cp /etc/kubernetes/admin.conf ~/.kube/
    cd ~/.kube
    sudo mv admin.conf config
    sudo service kubelet restart
    ```
+ 证书问题
    ```
    export KUBECONFIG=/etc/kubernetes/kubelet.conf
    kubectl get nodes
    ```