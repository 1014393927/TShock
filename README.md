<p align="center">
  <img src="https://tshock.co/newlogo.png" alt="TShock 中文"><br />
  <a href="https://travis-ci.org/NyxStudios/TShock"><img src="https://ci.appveyor.com/api/projects/status/cfpkv7rdscgwr1dd?svg=true" alt="编译状态"></a><br />
  <hr />
</p>

## 中文版本的说明

代码来自[NyxStudio的原Repository][ents]; 中文翻译使用[抗药又坚硬汉化组][sbmw]; OTAPI中文修改使用[Miyuu][miyuu].

若有汉化文本问题请提交issue或发送PR以帮助修复; 合作事项欢迎留言.

提交PR时请先阅读 [CONTRIBUTING.md](https://github.com/mistzzt/TShock/blob/adv-cn_dev/CONTRIBUTING.md) 并按照指明规范编写代码.

**注: 因为早期疏忽, 本汉化分支下有一些代码使用了空格而不是TAB. 后续会修复此问题.**

## 什么是 TShock

TShock 是 Terraria 的服务器模组(修改/mod), 使用C#编写, 并基于 [Terraria Server API][tsapi].

此mod提供一些原版服务器没有的新功能, 并支持多种扩展插件. 

## :star: 开服中文教程

https://tshock.readme.io/docs/getting-started-1

## 特性

* MySQL 数据库支持
* 权限系统
* 多玩家管理支持
* 反作弊
* 用户注册机制
* 预留位置(满员时允许特殊玩家进入)
* 玩家惩罚机制 (驱逐, 封禁, 禁言)
* 云存档模式(强制开荒)
* JSON格式的配置文件

## 社区/支持

* [英文官网/论坛](https://tshock.co/xf/)
* [对readme.io上的文档作贡献](https://tshock.readme.io/)

提交PR前请参阅Contributing文件.

## 下载途径

### 中文
* [Github Releases](https://github.com/mistzzt/TShock/releases)
* [最新编译版][ci] [![Build status](https://ci.appveyor.com/api/projects/status/cfpkv7rdscgwr1dd?svg=true)][ci]

### 英文
* [Github Releases](https://github.com/TShock/TShock/releases)
* [开发版更新](https://travis.tshock.co/)
* [官网插件资源](https://tshock.co/xf/index.php?resources/)
* [旧版本TShock](https://github.com/TShock/TShock/downloads)

[ci]: https://ci.appveyor.com/project/mistzzt/tshock/build/artifacts

[sbmw]: https://github.com/mst-mrh
[miyuu]:https://github.com/mst-mrh/Miyuu

[ents]: https://github.com/NyxStudios/TShock
[tsapi]: https://github.com/NyxStudios/TerrariaAPI-Server