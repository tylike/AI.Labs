# AI.Labs（云梯人工智能融合实验室）

AI.Labs 是一个开源项目，旨在融合最先进的人工智能技术，以提供一个功能强大的人工智能平台。该项目的核心在于集成多种人工智能服务，包括大型语言模型、语音识别和语音合成，以支持对话、语音交互和会议记录等功能。

![聊天界面](https://github.com/tylike/AI.Labs/blob/master/AI.Labs.Win/Images/AI.Labs.Chat.png)
![聊天设置](https://github.com/tylike/AI.Labs/blob/master/AI.Labs.Win/Images/ChatSettings.png)
![有声书籍](https://github.com/tylike/AI.Labs/blob/master/AI.Labs.Win/Images/AudioBook.png)

主要功能
## 1. 大型语言模型对话系统
### 1.1 OpenAI API 接口： 
项目集成了对 OpenAI 的 GPT-3.5 和 GPT-4 的支持，能够通过 OpenAI API 进行实时的智能对话。
使用了如下项目：https://github.com/betalgo/openai
### 1.2 本地语言模型服务器： 
通过 LMStudio 或 ChatGLM3-6B 在本地部署大型语言模型（LLM），使用与 OpenAI 类似的 API 进行交互，保证数据的私密性同时提高响应速度。
注模型、模型服务不限定，只要有openai风格的api支持就可以了。

下载地址：https://lmstudio.ai/ 
模型下载：https://huggingface.co/TheBloke 需要魔法 
模型下载：https://hf-mirror.com/TheBloke 免魔法 
模型下载：https://wisemodel.cn/models 免魔法 
模型下载：https://gpt4all.io/ 免魔法 

# 2. 语音识别 STT/ASR
## 2.1本地语音识别： 
### 2.1.1 利用 whisper.cpp 
实现本地语音识别功能，减少对外部服务的依赖并提高响应速度。

### 2.1.2 Azure 语音识别： 
也提供与 Azure 语音识别服务的集成，以利用其先进的云端语音识别技术。

## 2.2. STT/AS应用1:会议记录功能
利用项目的语音识别能力，AI.Labs 可以实现会议内容的实时记录，让信息归档和检索变得更加高效。
技术栈
项目使用了 DevExpress Application Framework，确保了用户界面的现代化和高度可定制性。



# 3. 语音转文本-TTS
## 3.1EdgeTTS 语音合成： 
使用 EdgeTTS 实现本地语音输出，免费。
使用了python项目edge-tts
需要在本地安装此python程序。
安装方法见:https://github.com/rany2/edge-tts
## 3.2Azure TTS 语音合成
自己可以去注册账户，每个月5小时免费或5M字符。
同时支持 Azure TTS 服务，以便用户可以选择云端的语音合成服务。
注册地址：https://portal.azure.com/




# 5.翻译与聊天的结合

DevExpress Application Framework介绍：
这是一个商业的框架，网址：http://www.devexpress.com
AI.Labs是基于C#（.Net6.0+）/SQLite数据库+XAF+OPENAI API+TTS+STT等一堆技术的组合。
尽量减少对python的依赖。但比如，edge-tts暂时没有办法摆脱的，还是得去调用。


二、问题与疑问
1.数据隐私与安全： 用户数据的隐私和安全如何保证？
如果您注意此选项，则可以使用LMStudio在本地（或局域网中）搭建私有模型。
支持多数gguf格式的模型或llama2.cpp的模型。
B站视频中介绍了此过程。注：中文视频。
https://www.bilibili.com/video/BV1Fc411D7eK/

2.多语言支持： 项目是否支持多种语言，以及如何处理不同语言之间的转换？
XAF支持多语音的方便扩展。会陆续推出各版本。
