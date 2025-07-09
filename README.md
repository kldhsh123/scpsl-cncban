# SCPSL-CNCBan 插件（联合封禁系统）

该插件用于接入 SCPSLCNC 联合封禁系统，实现多个 SCP: Secret Laboratory 服务器间的跨服封禁机制。通过 CNC 封禁系统，可有效遏制破坏社区环境的玩家，提升整体游戏体验。

⚠️ **注意：本仓库仅用于发布版本。插件源码同步维护不便，因此不会在 GitHub 上传全量源代码。如有需要可自行反编译查看，插件不包含任何恶意行为。**

---

## 📦 插件下载

请前往 [Releases](https://github.com/kldhsh123/scpsl-cncban/releases) 页面获取最新版本。

---

## ❓FAQ

### Q: 什么玩家需要被 CNC 封禁？

- 使用第三方作弊程序
- 在多个服务器中表现出极其恶劣的态度
- 对多个服务器进行蓄意破坏
- 其他严重损害公共游戏环境的行为

---

### Q: CNC 封禁的申诉流程是什么样的？

如您认为被误封，可以选择以下方式之一进行申诉：

- 发送邮件至：`admin@kldhsh.top`  
- 或加入 QQ 群：`729136722`  

---

### Q: 插件没有生效怎么办？

1. **开启插件调试模式**：该插件将封禁列表拉取日志设为 `Debug` 等级，以避免在 API 出现短时异常时输出大量错误。请开启调试模式以查看完整日志。
2. **检查 API 是否可访问**：确认服务器是否能成功连接 CNC 的 API 节点：  
   [https://api.kldhsh.top](https://api.kldhsh.top)  

---

## 🔒 权限说明

本插件不上传玩家隐私信息，不含任何后门逻辑，可使用反编译工具自行验证。如有任何安全疑虑，欢迎联系开发者反馈。

---

## 📮 联系我们

- CNC 封禁系统官网：[https://cncban.scpslgame.cn](https://cncban.scpslgame.cn)
- 封禁列表查看：[https://cncban.scpslgame.cn/bans](https://cncban.scpslgame.cn/bans)
- 加入我们 & 申诉 QQ 群：729136722
