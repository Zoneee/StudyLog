# Dokcer 术语
名词|注释
---|---
Container image (容器镜像)|包含创建容器所需的所有依赖项和信息的包 _(打包`Image`与`App`的可执行文件)_。 映像包括所有依赖项 _(例如框架)_，以及容器运行时使用的部署和执行配置。 通常情况下，映像派生自多个基础映像，这些基础映像是**堆叠在一起形成容器文件系统的层**。 **创建后，映像不可变。** 
Dockerfile|包含有关如何生成 Docker 映像的说明的文本文件。 **与批处理脚本相似**，首先第一行将**介绍基础映像，然后是关于安装所需程序、复制文件等操作的说明，直至获取所需的工作环境**。
Build|基于其 Dockerfile 提供的信息和上下文生成容器映像的操作，以及生成映像的文件夹中的其他文件。 可以使用 `docker build` **命令行**生成映像。
Container (容器)|Docker 映像的实例。 容器表示**单个应用程序、进程或服务的正在执行进程**。 它由 Docker 映像的内容、执行环境和一组标准指令组成。 在缩放服务时，可以从相同的映像创建多个容器实例。 或者，批处理作业可以从同一个映像创建多个容器，向每个实例传递不同的参数。
Volumes (卷)|**提供一个容器可以使用的可写文件系统**。 由于映像只可读取，而多数程序需要写入到文件系统，因此卷在容器映像顶部添加了一个可写层，这样程序就可以访问可写文件系统。 程序并不知道它正在访问的是分层文件系统，此文件系统就是往常的文件系统。 卷位于主机系统中，**由 Docker 管理。**
Tag (标记)|**用于识别同一映像的不同映像或版本**。 _(具体取决于版本号或目标环境)_。
Repository (repo)|相关的 Docker 映像集合，带有指示映像版本的标记 _(多个带有`Tag`的`Image`集合)_。 如 Linux 映像和 Windows 映像。
Registry (注册表)|**提供存储库访问权限的服务**。 大多数公共映像的默认注册表是 `Docker Hub` （归作为组织的 Docker 所有）。 注册表通常包含来自多个团队的存储库。 公司通常使用私有注册表来存储和管理其创建的映像。 另一个示例是 Azure 容器注册表。
Multi-arch image (多体系结构映像)|**根据 `Docker` 版本动态选择 `Image` 版本**。就多体系结构而言，它是一种根据运行 Docker 的平台简化相应映像选择的功能，例如，当 Dockerfile 从注册表请求基础映像 FROM mcr.microsoft.com/dotnet/core/sdk:2.2 时，实际上它会获得 2.2-sdk-nanoserver-1709、2.2-sdk-nanoserver-1803、2.2-sdk-nanoserver-1809 或 2.2-sdk-stretch，具体取决于运行 Docker 的操作系统版本。
Docker Hub|**`Docker` 私(公)有云**。 上传并使用映像的公共注册表。 Docker 中心提供 Docker 映像托管、公共或私有注册表，生成触发器和 Web 挂钩，以及与 GitHub 和 Bitbucket 集成。
Compose|一种**命令行工具**和**带有元数据的 `YAML` 文件格式**，用于**定义和运行多容器应用程序**。 基于多个映像定义单个应用程序，其中包含一个或多个 `.yml` 文件，这些文件可以根据环境替代值。 在创建定义之后，可以使用单个命令 (docker-compose up) 来部署整个多容器应用程序，**该命令在 `Docker` 主机上为每个映像创建容器。**
Cluster (群集)|Docker 主机集合像单一虚拟 Docker 主机一样公开，以便应用程序可以扩展到服务分布在群集中多个主机的多个实例。 Docker 群集可以使用 Kubernetes、Azure Service Fabric、Docker Swarm 和 Mesosphere DC/OS创建。
Orchestrator (业务流程协调程序)|简化群集和 Docker 主机管理的工具。 通过命令行界面 (CLI) 或图形用户界面，业务流程协调程序能够管理其映像、容器和主机。 可以管理容器网络、配置、负载均衡、服务发现、高可用性、Docker 主机配置等。 业务流程协调程序负责跨节点集合运行、分发、缩放和修复工作负荷。 通常情况下，业务流程协调程序产品是提供群集基础结构的同一产品，如 Kubernetes 和 Azure Service Fabric，以及市场中的其他产品/服务。

# Dotnet 支持的 Docker Image
图像|注释
---|---
mcr.microsoft.com/dotnet/core/aspnet:2.2	|ASP.NET Core，包含仅运行时和 ASP.NET Core 优化，适用于 Linux 和 Windows（多体系结构）
mcr.microsoft.com/dotnet/core/sdk:2.2	|.NET Core，包含 SDK，适用于 Linux 和 Windows（多体系结构）
mcr.microsoft.com/dotnet/core/runtime:2.2	|.NET Core 2.2 多体系结构：支持 Linux 和 Windows Nano Server，具体取决于 Docker 主机。

# 常用命令
## Docker
命令|标记|作用
---|---|---|
run||创建容器并运行进程
||-d|后台启动
||--name|设定名称
||-it|交互模式
||-p 80:81|宿主80端口映射到容器81端口
start/restart||运行已有容器
kill/stop||结束进程
build||根据 `Dockerfile` 构建镜像
||-t="name:tag"|设置镜像名称:版本
pull||拉取镜像
## Dockerfile
 **命令全部大写**
命令|格式|作用
---|---|---|
#||注释
FROM||选择基础镜像
|||**必须在首行**
MAINTAINER||开发者信息
CMD|CMD["命令","扩展"]|同CMD命令行。
|||建议参数传递为数组。
|||`run` 将覆盖 `Dockerfile` 中参数。
|||只能执行一条，必定为最后一条
ENTRYPOINT|同CMD|同CMD命令行。
|||建议参数传递为数组。
|||`run` 中参数，或接下来的 `CMD` 中参数将会传递到数组中合并执行。
|||`--entrypoint` 标志将覆盖 `Dockerfile` 中参数
WORKDIR||设定容器内进程工作目录
ENV|ENV RVM_PATH /home/rvm|设置环境变量 `RVM_PATH` 为 `/home/rvm`。**这很有用！**
|||设置的变量能够被 `Dockerfile` 中所有命令与容器进程使用
USER||镜像运行权限用户名。默认为root _linux_
VOLUME||向容器添加**卷**
|||支持跨容器共享
|||对卷(_内容_)修改及时生效。_如：添加文件等_
|||
ADD|ADD input output|从宿主机 input 目录将文件添加到容器 output 目录。
|||支持远程文件(_URL_)。
|||压缩包将自动解压。_不覆盖同名文件_
|||`input` 指定内容必须在构建目录或上下文中出现
|||`input` 指定内容以 “/” 结尾将判定为目录，否则则是文件
COPY|COPY input output|同ADD命令

