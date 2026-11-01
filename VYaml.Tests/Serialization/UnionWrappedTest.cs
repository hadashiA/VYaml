using System.Collections.Generic;
using NUnit.Framework;
using VYaml.Serialization;
using VYaml.Tests.TypeDeclarations;

namespace VYaml.Tests.Serialization
{
    [TestFixture]
    public class UnionWrappedTest : FormatterTestBase
    {
        [Test]
        public void Serialize()
        {
            Assert.That(Serialize<INotification>(new EmailNotification { To = "ops@example.com", Subject = "alert" }),
                Is.EqualTo("email:\n  to: ops@example.com\n  subject: alert\n"));
            Assert.That(Serialize<INotification>(new ConsoleNotification()), Is.EqualTo("console: {}\n"));
            Assert.That(Serialize<INotification?>(null), Is.EqualTo("null"));

            var config = new NotificationConfig
            {
                Notifications = new List<INotification> { new SlackNotification { Channel = "#alerts" }, new ConsoleNotification() },
            };
            Assert.That(Serialize(config), Is.EqualTo(
                "notifications:\n" +
                "- slack:\n" +
                "    channel: \"#alerts\"\n" +
                "- console: {}\n"));
        }

        [Test]
        public void Deserialize()
        {
            var config = Deserialize<NotificationConfig>(
                "notifications:\n" +
                "  - email: { to: ops@example.com, subject: 障害 }\n" +
                "  - slack:\n" +
                "      channel: \"#alerts\"\n" +
                "  - console: {}\n" +
                "  - console:\n" +
                "  - console: ~\n");

            Assert.That(config.Notifications, Has.Count.EqualTo(5));
            var email = (EmailNotification)config.Notifications[0];
            Assert.That(email.To, Is.EqualTo("ops@example.com"));
            Assert.That(email.Subject, Is.EqualTo("障害"));
            Assert.That(((SlackNotification)config.Notifications[1]).Channel, Is.EqualTo("#alerts"));
            // an empty value is an empty mapping
            Assert.That(config.Notifications[2], Is.InstanceOf<ConsoleNotification>());
            Assert.That(config.Notifications[3], Is.InstanceOf<ConsoleNotification>());
            Assert.That(config.Notifications[4], Is.InstanceOf<ConsoleNotification>());

            Assert.That(YamlSerializer.Deserialize<INotification?>(System.Text.Encoding.UTF8.GetBytes("~")), Is.Null);
        }

        [Test]
        public void Deserialize_EmptyValue_WithMembers()
        {
            // same as `slack: {}`: members not in the YAML take default(T); required members are still required
            var slack = (SlackNotification)Deserialize<INotification>("slack:\n");
            Assert.That(slack.Channel, Is.EqualTo(((SlackNotification)Deserialize<INotification>("slack: {}")).Channel));

            var ex = Assert.Throws<YamlSerializerException>(() => Deserialize<INotification>("webhook:\n"));
            Assert.That(ex!.Message, Does.Contain("'url'"));

            Assert.That(((WebhookNotification)Deserialize<INotification>("webhook: { url: https://example.com/hook }")).Url,
                Is.EqualTo("https://example.com/hook"));
        }

        [Test]
        public void Deserialize_Errors()
        {
            var unknown = Assert.Throws<YamlSerializerException>(() => Deserialize<INotification>("sms: {}\n"));
            Assert.That(unknown!.Message, Does.Contain("'sms'").And.Contain("'slack'").And.Contain("'console'"));

            var several = Assert.Throws<YamlSerializerException>(() => Deserialize<INotification>("slack: {}\nconsole: {}\n"));
            Assert.That(several!.Message, Does.Contain("exactly one key"));

            Assert.Throws<VYaml.Parser.YamlParserException>(() => Deserialize<INotification>("[slack]\n")); // not a mapping
            Assert.Throws<YamlSerializerException>(() => Deserialize<INotification>("? [slack]\n: {}\n")); // not a scalar key
            Assert.Throws<YamlSerializerException>(() => Deserialize<INotification>("{}\n"));
        }

        [Test]
        public void OverrideMemberAttributes()
        {
            var slack = (OverrideSlackNotification)Deserialize<OverrideNotificationBase>("slack:\n  priority: 2\n  channel: \"#alerts\"\n  note: n\n");
            Assert.That(slack.Target, Is.EqualTo("#alerts"));
            Assert.That(slack.Note, Is.EqualTo("n"));
            Assert.That(slack.Priority, Is.EqualTo(2));
            // the override stays at the position of the base declaration
            Assert.That(Serialize<OverrideNotificationBase>(slack), Is.EqualTo("slack:\n  channel: \"#alerts\"\n  note: n\n  priority: 2\n"));

            var email = (OverrideEmailNotification)Deserialize<OverrideNotificationBase>("email: { to: ops@example.com, note: n }");
            Assert.That(email.Target, Is.EqualTo("ops@example.com"));
            Assert.That(email.Note, Is.Null); // [YamlIgnore] on the override
            Assert.That(Serialize<OverrideNotificationBase>(email), Is.EqualTo("email:\n  to: ops@example.com\n  priority: 0\n"));
        }

        [Test]
        public void RoundTrip()
        {
            var value = new NotificationConfig
            {
                Notifications = new List<INotification>
                {
                    new EmailNotification { To = "ops@example.com", Subject = "alert" },
                    new SlackNotification { Channel = "#alerts" },
                    new ConsoleNotification(),
                    new WebhookNotification { Url = "https://example.com/hook" },
                },
            };
            var yaml = Serialize(value);
            var restored = Deserialize<NotificationConfig>(yaml);

            Assert.That(restored.Notifications, Has.Count.EqualTo(4));
            Assert.That(((EmailNotification)restored.Notifications[0]).Subject, Is.EqualTo("alert"));
            Assert.That(((SlackNotification)restored.Notifications[1]).Channel, Is.EqualTo("#alerts"));
            Assert.That(restored.Notifications[2], Is.InstanceOf<ConsoleNotification>());
            Assert.That(((WebhookNotification)restored.Notifications[3]).Url, Is.EqualTo("https://example.com/hook"));
        }
    }
}
