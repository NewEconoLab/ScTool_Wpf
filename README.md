# ScTool_Wpf
[English Document](README_EN.md)

ScTool �� SmartContract Tools �ļ�д����һ��NEO���ܺ�Լ���⹤�ߡ�

ScTool is "SmartContract Tools",is a set of tools for develop neo smartcontracts.

�ţ�����һ�׹��ߡ����������������ǰ�ˣ�ȫ������Դ��

er,It is a set of tools.Include servers and clients,and all opensourced.

һ�����������RemoteSharpContractBuilder,��ʱ�����SmartContractBrowser ��Ŀ�С������ڷ������ϱ���c#����

one of our server is called "RemoteSharpContractBuilder",you can find it in "SmartContractBrowser" project.it can compile c# code to avm on server.

��һ���������һ�����Ƶ� neo cli �ڵ�,��ʱ�����neo-gui-nel ��Ŀ�С�

another server is a special verison of neo-cli,you canfind it in "neo-gui-nel" project.it can make info for debug smartcontract.

Ŀǰ����ֻ������TestNet�ķ���API���������ǵķ������ǿ���ʹ�ã������������ֲ����������ϲ�����׹��ߣ����ǽ������Լ��������

for now,We just run our spec neo-cli on neo's chain "testnet",you can run your service if you need.

����ֱ�Ӳ�����ǰ��˷������ƣ�����Ϊ���󿪷�Web���߼��ĺ����ܸߡ�
�����Ѿ�׼����ΪWeb�������߼���API��

We use the C/S mode for this tools,beacuse a lot of people asked us to develop a web version tools.
We are ready for web tools now.we will do that later.
 

## Functions

���׹���Ŀǰ��Ҫ����������

now we have this functions:

1.C# online compiler C#���߱�����

![](image/pic1.png)
�����븴�ƽ����������������д�������ɳɹ������Դ�롢avm��abi�ļ���map�ļ� �ᱻ�����ڷ������ϡ�
�κ��˾��ɲ鿴��

just write your C# code here,or parse some text.and press the button "build scirpt".
you will got a result if it is successed.
the code\avm file\abi file\map file(map avm code -> src lines) will be saved on server.
everyone can look them.

�������������Զ��API���������������Կ���һ��Web�汾

this compiler depend on a remote api,so we can develop a web tool to compile c# code.
![](image/pic2.png)
���ǽ�����һ��������

this picture is a sample.

���գ����ǻ��ṩһ���׵�Web�������ߡ�

we will devlop a full set of web tools for Neo�� finially.

2.Debug tool,���ܺ�Լ���׵��Թ���

��һ�����������һ������Ľ��ף��鿴����ִ��ϸ��

another function is to check a invocation transaction,look the details in it.
![](image/pic3.png)
��ͼ���Ǹոշ�����һ�ʽ���

in this picture,we send a transaction

![](image/pic4.png)

Ȼ���ڵ��Թ��������뽻��ID���ɲ�ѯ����ִ�е�ϸ�ڡ�
����������а����������ǵ�C#online����������ĺ�Լ�����ǻ����Զ��������ص�Դ�룬����Ӧ��

what you need is just input the txid on the debug tools.
if some smartcontract script is called that is build by onlinebuilder,we can download the srccode for you too.

����������Ҫ��һ����Ϣ��
ִ��ջ�ͼ���ջ�ϵ�ֵ��ÿһ����ʱ��������
����ЩSyscall�����á�
Notify Log ��Щ�����ڻ��¡�

you can got everything,every detail.
every step about avm,every thing on stack and altstack.what syscall had be called.
you can see notify logs,everything.


3.And more ����������

ʵ���ϣ���Ҳ�����������׵��Թ���ȥ������ɫ�����棬�ռ�NEO�������ϲ�Ϊ����֪�����ܡ�
���ǿ���׼ȷ���ж�һ�����ܺ�Լ���׵���Ϊ����ȫ����������ʵִ�������

Acctruly,if you want developer a net spider or sth,this is useful too.
we can look everything in a invocation transaction.you wont miss anything on neo's chain.

## How to use
   
���빦�ܵ�ʹ�÷�����д����->�����밴ť->����������Զ������ڷ�����

how to compile:write code -> press button -> see result (Server will keep your code)

���Թ��ܵ�ʹ�÷���������txid->��load��ť->�����

how to debug:input the txid-> press button -> see what you got.

GoodDay.


