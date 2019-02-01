# StudyLog
+ 学习记录，将记录学习中产生的疑问、工作中出现的Bug以及相关的解决方案（如果有）

|时间|出处|问题|猜想|解决方案|
|：---：|：---|：---|：---|：----|
|2019年2月1日 14:47:15|《敏捷软件开发》第四章——测试|如何对模块进行适当的解耦？什么样的模块需要解耦合？当某个模块依靠于其他模块时，该模块需要解耦合吗？（例如数据消费模块依靠于数据生产模块）|该章主要讲述了如何从敏捷开发方向构建测试模块，引申到开发人员应该考虑到模块耦合度问题。`一个模块或应用程序具有可测试性，必须要对他进行解耦合。越是具有可测试性，耦合关系就越弱。全面的考虑验收测试和单元测试对软件的结构具有深远的正面影响`|难道说不通过数据生产模块将数据提供到数据消费模块，依旧保证数据消费模块正常运行就算是合格解耦吗？||

