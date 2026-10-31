using System.Collections.Generic;
using VYaml.Annotations;

namespace VYaml.Tests.TypeDeclarations
{
    [YamlObject]
    [YamlObjectUnion("circle", typeof(DiscriminatorCircle), DiscriminatorKey = "type")]
    [YamlObjectUnion("rect", typeof(DiscriminatorRect), DiscriminatorKey = "type")]
    public partial interface IDiscriminatorShape
    {
    }

    [YamlObject]
    public partial class DiscriminatorCircle : IDiscriminatorShape
    {
        public double Radius { get; set; }
    }

    [YamlObject]
    public partial class DiscriminatorRect : IDiscriminatorShape
    {
        public double Width { get; set; }
        public double Height { get; set; }
    }

    [YamlObject]
    [YamlObjectUnion("empty", typeof(DiscriminatorEmpty), DiscriminatorKey = "kind")]
    [YamlObjectUnion("named", typeof(DiscriminatorNamed), DiscriminatorKey = "kind")]
    public abstract partial class DiscriminatorBase
    {
    }

    [YamlObject]
    public partial class DiscriminatorEmpty : DiscriminatorBase
    {
    }

    [YamlObject]
    public partial class DiscriminatorNamed : DiscriminatorBase
    {
        public string Name { get; }
        public int Count { get; }

        public DiscriminatorNamed(string name, int count = 1)
        {
            Name = name;
            Count = count;
        }
    }

    [YamlObject]
    public partial class DiscriminatorContainer
    {
        public List<IDiscriminatorShape> Shapes { get; set; } = new();
        public DiscriminatorBase? Base { get; set; }
    }

    // the key of the target is given by [YamlMember] on an override property
    [YamlObject]
    [YamlObjectUnion("slack", typeof(OverrideSlackNotification), Style = YamlUnionStyle.Wrapped)]
    [YamlObjectUnion("email", typeof(OverrideEmailNotification), Style = YamlUnionStyle.Wrapped)]
    public abstract partial class OverrideNotificationBase
    {
        public abstract string Target { get; set; }
        public virtual string? Note { get; set; }
        public int Priority { get; set; }
    }

    [YamlObject]
    public partial class OverrideSlackNotification : OverrideNotificationBase
    {
        [YamlMember("channel")]
        public override string Target { get; set; } = "";
    }

    [YamlObject]
    public partial class OverrideEmailNotification : OverrideNotificationBase
    {
        [YamlMember("to")]
        public override string Target { get; set; } = "";

        [YamlIgnore]
        public override string? Note { get; set; }
    }

    // wrapped by a mapping with one key: `- slack: { channel: "#alerts" }`
    [YamlObject]
    [YamlObjectUnion("slack", typeof(SlackNotification), Style = YamlUnionStyle.Wrapped)]
    [YamlObjectUnion("email", typeof(EmailNotification), Style = YamlUnionStyle.Wrapped)]
    [YamlObjectUnion("webhook", typeof(WebhookNotification), Style = YamlUnionStyle.Wrapped)]
    [YamlObjectUnion("console", typeof(ConsoleNotification), Style = YamlUnionStyle.Wrapped)]
    public partial interface INotification
    {
    }

    /// <summary>
    /// Runs an action.
    /// </summary>
    [YamlObject]
    public partial class SlackNotification : INotification
    {
        public string Channel { get; set; } = "";
    }

    /// <summary>
    /// Sends an email.
    /// </summary>
    [YamlObject]
    public partial class EmailNotification : INotification
    {
        public string To { get; set; } = "";
        public string? Subject { get; set; }
    }

    [YamlObject]
    public partial class WebhookNotification : INotification
    {
        [YamlRequired]
        public string Url { get; set; } = "";
    }

    [YamlObject]
    public partial class ConsoleNotification : INotification
    {
    }

    [YamlObject]
    public partial class NotificationConfig
    {
        public List<INotification> Notifications { get; set; } = new();
    }

    // the key of each subtype is given by [YamlMember] on an override property
    [YamlObject]
    [YamlObjectUnion(typeof(OverrideKeyHug), RequiredKey = "hug")]
    [YamlObjectUnion(typeof(OverrideKeyWave), RequiredKey = "wave")]
    public abstract partial class OverrideKeyBase
    {
        public abstract string Name { get; set; }
        public virtual string? Note { get; set; }
        public int Power { get; set; }
    }

    [YamlObject]
    public partial class OverrideKeyHug : OverrideKeyBase
    {
        [YamlMember("hug")]
        public override string Name { get; set; } = "";
    }

    [YamlObject]
    public partial class OverrideKeyWave : OverrideKeyBase
    {
        [YamlMember("wave")]
        public override string Name { get; set; } = "";

        [YamlIgnore]
        public override string? Note { get; set; }
    }
}
