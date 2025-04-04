﻿using Intersect.Collections;
using NUnit.Framework;
using System.Reflection;

namespace Intersect.Network
{
    [TestFixture]
    public partial class PacketHandlerAttributeTests
    {
        [Test]
        public void TestGetPacketType_MethodInfo_null() =>
            Assert.Throws<ArgumentNullException>(() => PacketHandlerAttribute.GetPacketType(default(MethodInfo)));

        [Test]
        public void TestGetPacketType_MethodInfo_InvalidParameterCount() => Assert.Throws<ArgumentOutOfRangeException>(
            () => PacketHandlerAttribute.GetPacketType(
                typeof(TestPacketHandlerMethods).GetMethod(nameof(TestPacketHandlerMethods.HandleInvalidParameterCount))
            )
        );

        [Test]
        public void TestGetPacketType_MethodInfo_InvalidSenderParameterType() => Assert.Throws<ArgumentException>(
            () => PacketHandlerAttribute.GetPacketType(
                typeof(TestPacketHandlerMethods).GetMethod(
                    nameof(TestPacketHandlerMethods.HandleInvalidSenderParameterType)
                )
            )
        );

        [Test]
        public void TestGetPacketType_MethodInfo_InvalidPacketParameterType() => Assert.Throws<ArgumentException>(
            () => PacketHandlerAttribute.GetPacketType(
                typeof(TestPacketHandlerMethods).GetMethod(
                    nameof(TestPacketHandlerMethods.HandleInvalidPacketParameterType)
                )
            )
        );

        [Test]
        public void TestGetPacketType_MethodInfo_MismatchedAttribute() => Assert.Throws<ArgumentException>(
            () => PacketHandlerAttribute.GetPacketType(
                typeof(TestPacketHandlerMethods).GetMethod(
                    nameof(TestPacketHandlerMethods.HandleWithMismatchedAttribute)
                )
            )
        );

        [Test]
        public void TestGetPacketType_MethodInfo()
        {
            Assert.AreEqual(
                typeof(TestPacket),
                PacketHandlerAttribute.GetPacketType(
                    typeof(TestPacketHandlerMethods).GetMethod(nameof(TestPacketHandlerMethods.HandleNoAttribute))
                )
            );

            Assert.AreEqual(
                typeof(TestPacket),
                PacketHandlerAttribute.GetPacketType(
                    typeof(TestPacketHandlerMethods).GetMethod(nameof(TestPacketHandlerMethods.HandleWithAttribute))
                )
            );

            Assert.AreEqual(
                typeof(TestPacket2),
                PacketHandlerAttribute.GetPacketType(
                    typeof(TestPacketHandlerMethods).GetMethod(nameof(TestPacketHandlerMethods.HandleWithAttribute2))
                )
            );
        }

        [Test]
        public void TestGetPacketType_Type_null() =>
            Assert.Throws<ArgumentNullException>(() => PacketHandlerAttribute.GetPacketTypes(default(Type)));

        [Test]
        public void TestGetPacketType_Type_NotPacketHandlerImplementation() => Assert.Throws<ArgumentException>(
            () => PacketHandlerAttribute.GetPacketTypes(typeof(TestClassNotPacketHandler))
        );

        [Test]
        public void TestGetPacketType_Type_InvalidPacketType() => Assert.Throws<ArgumentException>(
            () => PacketHandlerAttribute.GetPacketTypes(typeof(TestClassPacketHandlerInvalidPacketType))
        );

        [Test]
        public void TestGetPacketType_Type_MismatchedAttribute() => Assert.Throws<ArgumentException>(
            () => PacketHandlerAttribute.GetPacketTypes(typeof(TestClassPacketHandlerWithMismatchedAttribute))
        );

        [Test]
        public void TestGetPacketType_Type()
        {
            Assert.Multiple(
                () =>
                {
                    Assert.That(
                        PacketHandlerAttribute.GetPacketTypes(typeof(TestClassPacketHandlerNoAttribute)),
                        Is.EquivalentTo(
                            new[]
                            {
                                typeof(TestPacket),
                            }
                        )
                    );

                    Assert.That(
                        PacketHandlerAttribute.GetPacketTypes(typeof(TestClassPacketHandlerWithAttribute)),
                        Is.EquivalentTo(
                            new[]
                            {
                                typeof(TestPacket),
                            }
                        )
                    );

                    Assert.That(
                        PacketHandlerAttribute.GetPacketTypes(typeof(TestClassPacketHandlerWithAttribute2)),
                        Is.EquivalentTo(
                            new[]
                            {
                                typeof(TestPacket2),
                            }
                        )
                    );
                }
            );
        }

        [Test]
        public void TestGetHandlerInterface_Type_null() =>
            Assert.Throws<ArgumentNullException>(() => PacketHandlerAttribute.GetHandlerInterfaces(null));

        [Test]
        public void TestGetHandlerInterface_Type_NotHandler() =>
            Assert.That(
                () => PacketHandlerAttribute.GetHandlerInterfaces(typeof(TestClassNotPacketHandler)),
                Throws.ArgumentException
            );

        [Test]
        public void TestGetHandlerInterface_Type()
        {
            Assert.Multiple(
                () =>
                {
                    Assert.That(
                        new []{typeof(IPacketHandler<TestPacket>)},
                        Is.EquivalentTo(PacketHandlerAttribute.GetHandlerInterfaces(typeof(TestClassPacketHandlerNoAttribute)))
                    );

                    Assert.That(
                        new[]{typeof(IPacketHandler<TestPacket>)},
                        Is.EquivalentTo(PacketHandlerAttribute.GetHandlerInterfaces(typeof(TestClassPacketHandlerWithAttribute)))
                    );

                    Assert.That(
                        new[]{typeof(IPacketHandler<TestPacket2>)},
                        Is.EquivalentTo(PacketHandlerAttribute.GetHandlerInterfaces(typeof(TestClassPacketHandlerWithAttribute2)))
                    );

                    Assert.That(
                        new[]{typeof(IPacketHandler<TestPacket>)},
                        Is.EquivalentTo(PacketHandlerAttribute.GetHandlerInterfaces(typeof(TestClassPacketHandlerWithMismatchedAttribute)))
                    );

                    // This specific handler is disallowed in the registry and filtered out elsewhere,
                    // but PacketHandlerAttribute.GetHandlerInterface() allowing it is acceptable.
                    Assert.That(
                        new[]{typeof(IPacketHandler<IPacket>)},
                        Is.EquivalentTo(PacketHandlerAttribute.GetHandlerInterfaces(typeof(TestClassPacketHandlerInvalidPacketType)))
                    );
                }
            );
        }

        [Test]
        public void TestIsValidHandler_MethodInfo_null() =>
            Assert.Throws<ArgumentNullException>(() => PacketHandlerAttribute.GetPacketType(default(MethodInfo)));

        [Test]
        public void TestIsValidHandler_MethodInfo_NoAttribute() => Assert.IsTrue(
            PacketHandlerAttribute.IsValidHandler(
                typeof(TestPacketHandlerMethods).GetMethod(nameof(TestPacketHandlerMethods.HandleNoAttribute))
            )
        );

        [Test]
        public void TestIsValidHandler_MethodInfo_MismatchedAttribute() => Assert.IsFalse(
            PacketHandlerAttribute.IsValidHandler(
                typeof(TestPacketHandlerMethods).GetMethod(
                    nameof(TestPacketHandlerMethods.HandleWithMismatchedAttribute)
                )
            )
        );

        [Test]
        public void TestIsValidHandler_MethodInfo_InvalidParameterCount() => Assert.IsFalse(
            PacketHandlerAttribute.IsValidHandler(
                typeof(TestPacketHandlerMethods).GetMethod(nameof(TestPacketHandlerMethods.HandleInvalidParameterCount))
            )
        );

        [Test]
        public void TestIsValidHandler_MethodInfo_InvalidSenderParameterType() => Assert.IsFalse(
            PacketHandlerAttribute.IsValidHandler(
                typeof(TestPacketHandlerMethods).GetMethod(
                    nameof(TestPacketHandlerMethods.HandleInvalidSenderParameterType)
                )
            )
        );

        [Test]
        public void TestIsValidHandler_MethodInfo_InvalidPacketParameterType() => Assert.IsFalse(
            PacketHandlerAttribute.IsValidHandler(
                typeof(TestPacketHandlerMethods).GetMethod(
                    nameof(TestPacketHandlerMethods.HandleInvalidPacketParameterType)
                )
            )
        );

        [Test]
        public void TestIsValidHandler_MethodInfo()
        {
            Assert.IsTrue(
                PacketHandlerAttribute.IsValidHandler(
                    typeof(TestPacketHandlerMethods).GetMethod(nameof(TestPacketHandlerMethods.HandleWithAttribute))
                )
            );

            Assert.IsTrue(
                PacketHandlerAttribute.IsValidHandler(
                    typeof(TestPacketHandlerMethods).GetMethod(nameof(TestPacketHandlerMethods.HandleWithAttribute2))
                )
            );
        }

        [Test]
        public void TestIsValidHandler_Type_null() =>
            Assert.Throws<ArgumentNullException>(() => PacketHandlerAttribute.GetPacketTypes(default(Type)));

        [Test]
        public void TestIsValidHandler_Type_NoAttribute() => Assert.IsTrue(
            PacketHandlerAttribute.IsValidHandler(typeof(TestClassPacketHandlerNoAttribute))
        );

        [Test]
        public void TestIsValidHandler_Type_MismatchedAttribute() => Assert.IsFalse(
            PacketHandlerAttribute.IsValidHandler(typeof(TestClassPacketHandlerWithMismatchedAttribute))
        );

        [Test]
        public void TestIsValidHandler_Type_InvalidPacketType() => Assert.IsFalse(
            PacketHandlerAttribute.IsValidHandler(typeof(TestClassPacketHandlerInvalidPacketType))
        );

        [Test]
        public void TestIsValidHandler_Type()
        {
            Assert.IsTrue(PacketHandlerAttribute.IsValidHandler(typeof(TestClassPacketHandlerWithAttribute)));

            Assert.IsTrue(PacketHandlerAttribute.IsValidHandler(typeof(TestClassPacketHandlerWithAttribute2)));
        }
    }

    internal static partial class TestPacketHandlerMethods
    {
        public static bool HandleInvalidParameterCount(IPacketSender packetSender) =>
            throw new NotImplementedException();

        public static bool HandleInvalidSenderParameterType(object sender, TestPacket packet) =>
            throw new NotImplementedException();

        public static bool HandleInvalidPacketParameterType(IPacketSender packetSender, IPacket packet) =>
            throw new NotImplementedException();

        public static bool HandleNoAttribute(IPacketSender packetSender, TestPacket packet) =>
            throw new NotImplementedException();

        [PacketHandler(typeof(TestPacket))]
        public static bool HandleWithAttribute(IPacketSender packetSender, TestPacket packet) =>
            throw new NotImplementedException();

        [PacketHandler(typeof(TestPacket2))]
        public static bool HandleWithAttribute2(IPacketSender packetSender, TestPacket2 packet) =>
            throw new NotImplementedException();

        [PacketHandler(typeof(TestPacket2))]
        public static bool HandleWithMismatchedAttribute(IPacketSender packetSender, TestPacket packet) =>
            throw new NotImplementedException();
    }

    internal sealed partial class TestClassPacketHandlerNoAttribute : IPacketHandler<TestPacket>
    {
        #region Implementation of IPacketHandler

        /// <inheritdoc />
        public bool Handle(IPacketSender packetSender, TestPacket packet) => throw new NotImplementedException();

        /// <inheritdoc />
        public bool Handle(IPacketSender packetSender, IPacket packet) => throw new NotImplementedException();

        #endregion
    }

    [PacketHandler(typeof(TestPacket))]
    internal sealed partial class TestClassPacketHandlerWithAttribute : IPacketHandler<TestPacket>
    {
        #region Implementation of IPacketHandler

        /// <inheritdoc />
        public bool Handle(IPacketSender packetSender, TestPacket packet) => throw new NotImplementedException();

        /// <inheritdoc />
        public bool Handle(IPacketSender packetSender, IPacket packet) => throw new NotImplementedException();

        #endregion
    }

    [PacketHandler(typeof(TestPacket2))]
    internal sealed partial class TestClassPacketHandlerWithAttribute2 : IPacketHandler<TestPacket2>
    {
        #region Implementation of IPacketHandler

        /// <inheritdoc />
        public bool Handle(IPacketSender packetSender, TestPacket2 packet) => throw new NotImplementedException();

        /// <inheritdoc />
        public bool Handle(IPacketSender packetSender, IPacket packet) => throw new NotImplementedException();

        #endregion
    }

    [PacketHandler(typeof(TestPacket2))]
    internal sealed partial class TestClassPacketHandlerWithMismatchedAttribute : IPacketHandler<TestPacket>
    {
        #region Implementation of IPacketHandler

        /// <inheritdoc />
        public bool Handle(IPacketSender packetSender, TestPacket packet) => throw new NotImplementedException();

        /// <inheritdoc />
        public bool Handle(IPacketSender packetSender, IPacket packet) => throw new NotImplementedException();

        #endregion
    }

    internal sealed partial class TestClassNotPacketHandler
    {
    }

    internal sealed partial class TestClassPacketHandlerInvalidPacketType : IPacketHandler<IPacket>
    {
        #region Implementation of IPacketHandler

        /// <inheritdoc />
        public bool Handle(IPacketSender packetSender, IPacket packet) => throw new NotImplementedException();

        /// <inheritdoc />
        bool IPacketHandler.Handle(IPacketSender packetSender, IPacket packet) => throw new NotImplementedException();

        #endregion
    }

    internal sealed partial class TestPacket : IPacket
    {
        #region Implementation of IDisposable

        /// <inheritdoc />
        public void Dispose() => throw new NotImplementedException();

        #endregion

        #region Implementation of IPacket

        /// <inheritdoc />
        public byte[] Data { get; }

        /// <inheritdoc />
        public bool IsValid { get; }

        /// <inheritdoc />
        public long ReceiveTime { get; set; }

        /// <inheritdoc />
        public long ProcessTime { get; set; }

        /// <inheritdoc />
        public Dictionary<string, SanitizedValue<object>> Sanitize() => throw new NotImplementedException();

        #endregion
    }

    internal sealed partial class TestPacket2 : IPacket
    {
        #region Implementation of IDisposable

        /// <inheritdoc />
        public void Dispose() => throw new NotImplementedException();

        #endregion

        #region Implementation of IPacket

        /// <inheritdoc />
        public byte[] Data { get; }

        /// <inheritdoc />
        public bool IsValid { get; }

        /// <inheritdoc />
        public long ReceiveTime { get; set; }

        /// <inheritdoc />
        public long ProcessTime { get; set; }

        /// <inheritdoc />
        public Dictionary<string, SanitizedValue<object>> Sanitize() => throw new NotImplementedException();

        #endregion
    }
}
