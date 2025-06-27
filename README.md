# EasyTemplate.Blazor.Web 简单的Blazor后台管理系统模板

#### 介绍

简易后台系统框架，使用Blazor Server编写，前后端不分离，单体架构
该仓库主要目的：
1.  尽可能轻量化，减少分层与多重调用
2.  完整展示如何使用Ant Design基于Server端做开发
3.  单人即可完成管理系统的研发

#### 软件架构

.Net8 + Ant Design + Blazor Server + Sqlsugar

#### 安装教程

1.  目前仅基于mysql开发测试，在 EasyTemplate.Tool/Configuration/DataBase.json，修改ConnectionString后即可
2.  将EasyTemplate.Blazor.Web设置为启动项目后，运行项目
3.  登录过期时间通过 EasyTemplate.Tool/Configuration/App.json 的 App:ExpiredTime 修改
4.  卷影复制配置在 EasyTemplate.Blazor.Web/web.config/handlerSetting 节点中，解决了发布时占用问题，但硬盘会因此被占用，介意的可以将该节点删除，或是配置enableShadowCopy为false

#### 项目说明

1.  EasyTemplate.Blazor.Web：项目主入口，将该项目设置为启动项目
2.  EasyTemplate.Page：在这里添加页面。如果业务不复杂，也可以删除该项目，将所有页面写在 EasyTemplate.Blazor.Web/Components/Pages 中
2.  EasyTemplate.Service：在这里添加Api。接口使用dynamic controller动态生成，如果要查看swagger json，直接在端口后复制这一段后访问：/swagger/all/swagger.json
3.  EasyTemplate.Tool：在这里添加实体。配置文件 Configuration + 实体 Entity + 工具类 Util

#### 注意

1.  实现了简单的用户-角色-菜单-部门对应关系，角色为单角色
2.  页面权限验证使用了自定义的方式，没有使用Blazor自带的 AutorizeView 和 [Autorize]
3.  Api权限验证使用了JWT，过滤器的定义独立于页面验证，如果需要修改，在 EasyTemplate.Service/Common/BaseFilter 
4.  数据库使用了Sqlsugar的实体生成数据库表功能，无需导入库。只需要先设置ConnectionString，再打开 EasyTemplate.Tool/Util/Sql.cs 中的 InitDatabase 方法即可，目前默认打开，如果已有库表和数据，可以注掉
5.  虽然有redis支持，但本项目基本未使用缓存功能，用户信息仍然是从数据库中读取，如有需要可以自行修改

#### 组件参考

https://antblazor.com/zh-CN