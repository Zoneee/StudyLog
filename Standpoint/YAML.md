# 简介
`YAML` 可以理解为类似 `Markdown` 语言。  
用来记录 `k8s`、`Docker` 等服务配置信息，作用等同于 `appsetting.json`

# 语法规则
大小写敏感  
使用缩进表示层级关系。与`Python`相同  
不能使用 `Tab` ，必须使用空格对齐  
`#` 注释当前行  
`---` 用来分组。_作用不明，配合命令中的 `-f` 标记使用吗？_

# 结构类型
+ Maps
  + `Key/Value` 
+ Lists
  + 在 `Value` 起始处用 `-` 标记
  + 对应`Json`中`Array`
  + 例：
    ```Json
    {Key:[{obj1},{obj2}}
    ```


