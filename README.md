# ScTool_Wpf
* 此项目已废弃 The project has been discarded
* 合约调试功能单独抽出为neondebug项目。smartcontract debug function has moved to project "neondebug".
* 其他功能抽出为neo-thinsdk-cs项目。Other functions has moded to project"neo-thinsdk-cs".
* 将于数日后删除仓库.this project will be remove in few days.

[English Document](README_EN.md)

ScTool 是 SmartContract Tools 的简写，是一套NEO智能合约调测工具。

ScTool in "SmartContract Tools" is a set of tools for develop NEO smart contracts.

嗯，他是一套工具。包含两组服务器和前端，全部都开源。

It includes servers and clients, and all open-sourced.

一组服务器叫做RemoteSharpContractBuilder,暂时存放在SmartContractBrowser 项目中。他能在服务器上编译c#代码

One of our server is called "RemoteSharpContractBuilder". You can find it in the "SmartContractBrowser" project. It can compile C# code to .avm on the server.

另一组服务器是一个定制的 neo cli 节点,暂时存放在neo-gui-nel 项目中。

Another server is a special verison of neo-cli. You can find it in the "neo-gui-nel" project. It provides info to debug smart contracts.

目前我们只部署了TestNet的服务API，由于我们的服务器是开发使用，经常会做各种操作。如果你喜欢这套工具，我们建议你自己部署服务。

For now, we just run our neo-cli on NEO's testnet. But you can also run it on your own chain.

我们直接采用了前后端分离的设计，是因为请求开发Web工具集的呼声很高。
我们已经准备好为Web开发工具集的API。

We use the C/S mode for this tools, as lots of people asked us to develop web tools. This effort will follow soon.

## Functions

这套工具目前主要有两个功能

We have the following function:

1. C# online compiler C#在线编译器

![](image/pic1.png)
将代码复制进来，或者在这里编写。若生成成功，你的源码、avm、abi文件、map文件 会被保存在服务器上。
任何人均可查看。

Just write or paste your C# code here and press the "build scirpt" button.
If it is successed, you will be provided with the result. 
The code\avm file\abi file\map file (map avm code -> src lines) will be publically saved on the server.

这个编译器基于远程API工作，所以他可以开发一个Web版本

This compiler depend on a remote api, so we can develop a web tool to compile C# code.
![](image/pic2.png)
我们建立了一个范例。

This image is an example.

最终，我们会提供一整套的Web开发工具。

We will eventually devlop a full set of web tools for NEO.

2.Debug tool, 智能合约交易调试工具

另一个功能是针对一个具体的交易，查看他的执行细节

Another function is to check a invocation transaction. Take a look at the defails.
![](image/pic3.png)
如图我们刚刚发起了一笔交易

In this picture, we send a transaction.

![](image/pic4.png)

然后在调试工具中输入交易ID即可查询交易执行的细节。
如果调用链中包含你用我们的C#online编译器编译的合约，我们还能自动帮你下载到源码，并对应。

You just need to input the txid to the debug tools.
If some smart contract script is called that is build by the online builder, one can download the source code as well.

这里有你需要的一切信息。
执行栈和计算栈上的值在每一步的时候的情况。
有哪些Syscall被调用。
Notify Log 这些都不在话下。

You can get everything in detail.
Every step in the .avm, every thing on the stack and altstack that the syscall had called.
You can also see notify logs.

3.And more 其他可能性

实际上，你也可以利用这套调试工具去开发特色的爬虫，收集NEO区块链上不为人所知的秘密。
我们可以准确的判定一个智能合约交易的行为，完全基于链上真实执行情况。

If you want develope a net spider or sth, this is useful too.
One can look at everything in a invocation transaction. You wont miss anything on the NEO blockchain.

## Usage
   
编译功能的使用方法：写代码->按编译按钮->看到结果并自动保存在服务器

How to compile: 
write code -> press button -> see the result (the server will keep your code)

调试功能的使用方法：输入txid->按load按钮->看结果

How to debug:
input the txid -> press button -> see what you got.

Good day.

