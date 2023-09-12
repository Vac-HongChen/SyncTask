# SyncTask
SyncTask:专门为SyncThread.WaitMainThread创建的安全Task
什么是SyncTask呢?
已知一个Task可以进行await 同时可以对外提供一个SetResult方法
正常情况下 在执行await task之后
就会触发UnsafeOnCompleted函数或者OnCompleted函数
目前OnCompleted函数还没遇见过
触发UnsafeOnCompleted函数是什么意思呢,就是将await task后边的代码行封装成为一个方法传递进来
然后在主线程调用SetResult,顺势执行通过UnsafeOnCompleted函数传递过来的continuation
上述,就可以将一个本来在异步线程的代码行转换到主线程处理

但是!!!
由于异步线程的先后执行顺序不固定,就会出现先调用SetResult,后调用UnsafeOnCompleted的情况
常规做法是调用UnsafeOnCompleted的时候检测一下是否该task是否已经完成,如果完成则直接执行方法
这样就会导致UnsafeOnCompleted函数传递过来的continuation还是在异步线程执行
为了解决这种问题 创建了SyncTask来进行解决


## 运行结果
2023/9/12 18:18:22 线程:1 创建Task index:0 tmp:0
2023/9/12 18:18:22 线程:4 执行Task index:0 tmp:0
2023/9/12 18:18:22 线程:1 创建Task index:1 tmp:0
2023/9/12 18:18:22 线程:5 执行Task index:1 tmp:0
2023/9/12 18:18:23 线程:5 Task结束 异步线程 index:1 tmp:0
2023/9/12 18:18:23 线程:4 Task结束 异步线程 index:0 tmp:0
2023/9/12 18:18:23 线程:1 Task结束 回到主线程 index:1 tmp:0
2023/9/12 18:18:23 线程:1 创建Task index:1 tmp:1
2023/9/12 18:18:23 线程:7 执行Task index:1 tmp:1
2023/9/12 18:18:23 线程:1 Task结束 回到主线程 index:0 tmp:0
2023/9/12 18:18:23 线程:1 创建Task index:0 tmp:1
2023/9/12 18:18:23 线程:4 执行Task index:0 tmp:1
2023/9/12 18:18:24 线程:4 Task结束 异步线程 index:0 tmp:1
2023/9/12 18:18:24 线程:7 Task结束 异步线程 index:1 tmp:1
2023/9/12 18:18:24 线程:1 Task结束 回到主线程 index:0 tmp:1
2023/9/12 18:18:24 线程:1 Task结束 回到主线程 index:1 tmp:1