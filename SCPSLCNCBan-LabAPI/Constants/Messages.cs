namespace SCPSLCNCBan_LabAPI.Constants
{
    /// <summary>
    /// 消息模板常量
    /// </summary>
    public static class Messages
    {
        /// <summary>
        /// 基础踢出消息模板
        /// </summary>
        public const string KICK_MESSAGE = 
            "封禁原因:{reason}\n" + 
            "封禁开始时间:{starttime}\n" + 
            "如有异议，请前往CNCBAN封禁网站联系申诉\n" + 
            "- cncban.scpslgame.cn -";

        /// <summary>
        /// IP被封禁消息
        /// </summary>
        public const string IP_BANNED_MESSAGE = "您的ip已被永久封禁\n如果您正在使用加速器，请尝试关闭加速器或更换节点";

        /// <summary>
        /// ID被封禁消息
        /// </summary>
        public const string ID_BANNED_MESSAGE = "您的{platform}账号已被永久封禁";

        /// <summary>
        /// ID和IP都被封禁消息
        /// </summary>
        public const string BOTH_BANNED_MESSAGE = "您的{platform}账号和IP地址均已被永久封禁";

        /// <summary>
        /// CNBan ID封禁消息
        /// </summary>
        public const string CNBAN_BANNED_MESSAGE = "您已被CNBan永久封禁\n您可能在游戏内外使用了包括但不限于外挂，恶意BUG等非法手段，遭到举报并核实后封禁，永久无法加入任何国服联盟封禁的服务器\n如有疑问或申诉请访问: ac.cnscpsl.cn";

        /// <summary>
        /// CNBan IP封禁消息
        /// </summary>
        public const string CNBAN_IP_BANNED_MESSAGE = "您的ip已被CNBan永久封禁\n您可能在游戏内外使用了包括但不限于外挂，恶意BUG等非法手段，遭到举报并核实后封禁，永久无法加入任何国服联盟封禁的服务器\n如有疑问或申诉请访问: ac.cnscpsl.cn\n如果您正在使用加速器，请尝试关闭加速器或更换节点";

        /// <summary>
        /// CNBan ID和IP都被封禁消息
        /// </summary>
        public const string CNBAN_BOTH_BANNED_MESSAGE = "您已被CNBan永久封禁\n您可能在游戏内外使用了包括但不限于外挂，恶意BUG等非法手段，遭到举报并核实后封禁，永久无法加入任何国服联盟封禁的服务器\n如有疑问或申诉请访问: ac.cnscpsl.cn";

        /// <summary>
        /// 敏感词检测消息
        /// </summary>
        public const string KEYWORD_BANNED_MESSAGE = "您的{platform}昵称中包含了极度敏感的词汇，出于安全考虑，您无法加入此服务器，\n请修改您的昵称并重启游戏后尝试加入，如有疑问请联系服务器管理员";
    }
}
