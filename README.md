# SCPSL-CNCBan 插件（中国社区联合封禁系统）

该插件用于接入 SCPSLCNC 联合封禁系统，实现多个 SCP: Secret Laboratory 服务器间的跨服封禁机制。通过 CNC 封禁系统，可有效遏制破坏社区环境的玩家，提升整体游戏体验。

## ✨ 主要功能

- 🚫 **多重封禁检查**：支持 CNCBAN + CNBan 双重封禁系统
- 🔍 **敏感词过滤**：自动检测并阻止包含敏感词的玩家昵称
- 🌐 **IP段封禁**：支持IP段和精确IP封禁检查
- 📝 **详细日志**：完整的封禁记录和调试信息

---
> 为了高效阻止外挂，我们默认启用同步CNBan封禁列表，您无需将此插件和CNBan插件同时启用。

## 📦 插件下载

请前往 [Releases](https://github.com/kldhsh123/scpsl-cncban/releases) 页面获取最新版本。

## ⚙️ 配置说明

### 主要配置项

```yaml
# 基础设置
debug: false                          # 是否启用调试模式
enable_ip_ban: true                   # 是否启用CNCBAN IP封禁检查
enable_log_cooldown: true             # 是否启用日志冷却（避免重复记录）

# CNBan设置
sync_cn_ban_players: true             # 是否同步CNBan封禁列表
enable_cn_ban_ip_ban: true            # 是否启用CNBan IP段封禁检查

# 敏感词设置
enable_cloud_keywords: true           # 是否启用云端敏感词
```

### 调试模式

启用 `debug: true` 后，插件将输出详细的调试信息：
- 封禁列表更新状态
- 玩家检查过程
- 匹配结果详情
- 错误诊断信息

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

1. **开启插件调试模式**：在配置文件中设置 `debug: true`，查看详细的调试信息
2. **检查封禁列表更新**：观察日志中的 "封禁列表已更新" 消息
3. **验证网络连接**：确认服务器能访问封禁API

---

### Q: 支持哪些封禁类型？

- **Steam ID 封禁**：基于Steam账号的封禁
- **IP段封禁**：如 `192.168.1.*` 的范围封禁
- **敏感词封禁**：基于玩家昵称的关键词过滤

---

## � 相关项目

- **CNBan 封禁列表**：[https://github.com/Manghui/CNBanList](https://github.com/Manghui/CNBanList)
  - 本插件默认集成此封禁系统

## �📮 联系我们

- CNC 封禁系统官网：[https://cncban.scpslgame.cn](https://cncban.scpslgame.cn)
- 封禁列表查看：[https://cncban.scpslgame.cn/bans](https://cncban.scpslgame.cn/bans)
- 加入我们 & 申诉 QQ 群：729136722
