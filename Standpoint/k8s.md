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

# 环境配置
+ OS：CentOS-7
+ 修改网络环境
  + vi /etc/sysconfig/network-scripts/ifcfg-eth0 
+ 安装环境 _国内可能无法直连k8s，见`设置阿里云镜像`_
  + yum install -y kubelet kubeadm kubectl docker
+  设置阿里云镜像
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
+ 禁用 swap
  + sudo swapoff -a
+ 关闭防火墙
  + systemctl stop firewalld.service _关闭_
  + systemctl disable firewalld.service _禁用自启_
+ k8s初始化 _`kubeadm config images pull` 失败见 [国内获取k8s image](#%e5%9b%bd%e5%86%85%e8%8e%b7%e5%8f%96k8s-image)_
  + kubeadm init
+ 重置+启动+初始化
  + kubeadm reset && systemctl start  kubelet && kubeadm init 
+ 屏蔽Error级错误
  + --ignore-preflight-errors=all


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
3. 预先 `pull` 镜像
   ```
   kubeadm config images pull --config kubeadm.conf
   ```
   

## 方法二：docker+tag
1. pull 镜像
   + tag 需要根据kubeadm版本改动，具体版本可以在配置文件中查询输出，详见[输出配置文件](#%e6%96%b9%e6%b3%95%e4%b8%80%e4%bf%ae%e6%94%b9%e9%85%8d%e7%bd%ae%e6%96%87%e4%bb%b6)
    ```
    docker pull gcr.azk8s.cn/google_containers/kube-apiserver-amd64:v1.15.2
    docker pull gcr.azk8s.cn/google_containers/kube-controller-manager-amd64:v1.15.2
    docker pull gcr.azk8s.cn/google_containers/kube-scheduler-amd64:v1.15.2
    docker pull gcr.azk8s.cn/google_containers/kube-proxy-amd64:v1.15.2
    docker pull gcr.azk8s.cn/google_containers/etcd-amd64:3.1.12
    docker pull gcr.azk8s.cn/google_containers/pause-amd64:3.1
    ```
2. tag 镜像
   + 利用docker检查机制欺骗kubeadm
   ```
    docker tag gcr.azk8s.cn/google_containers/kube-apiserver-amd64:v1.15.2 k8s.gcr.io/kube-apiserver-amd64:v1.15.2
    docker tag gcr.azk8s.cn/google_containers/kube-scheduler-amd64:v1.15.2 k8s.gcr.io/kube-scheduler-amd64:v1.15.2
    docker tag gcr.azk8s.cn/google_containers/kube-controller-manager-amd64:v1.15.2 k8s.gcr.io/kube-controller-manager-amd64:v1.15.2
    docker tag gcr.azk8s.cn/google_containers/kube-proxy-amd64:v1.15.2 k8s.gcr.io/kube-proxy-amd64:v1.15.2
    docker tag gcr.azk8s.cn/google_containers/etcd-amd64:3.1.12 k8s.gcr.io/etcd-amd64:3.1.12
    docker tag gcr.azk8s.cn/google_containers/pause-amd64:3.1 k8s.gcr.io/pause-amd64:3.1
   ```
3. 启动 
   ```
   sudo kubeadm init --kubernetes-version=v1.15.2 --feature-gates=CoreDNS=true --pod-network-cidr=192.168.129/24
   ```