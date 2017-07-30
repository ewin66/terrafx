// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

using System.Runtime.InteropServices;
using NUnit.Framework;

namespace TerraFX.Interop.UnitTests
{
    /// <summary>Provides validation of the <see cref="LARGE_INTEGER" /> struct.</summary>
    public static class LARGE_INTEGERTests
    {
        /// <summary>Validates that the layout of the <see cref="LARGE_INTEGER" /> struct is <see cref="LayoutKind.Explicit" />.</summary>
        [Test]
        public static void IsLayoutExplicitTest()
        {
            Assert.That(typeof(LARGE_INTEGER).IsExplicitLayout, Is.True);
        }

        /// <summary>Validates that the size of the <see cref="LARGE_INTEGER" /> struct is correct.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(Marshal.SizeOf<LARGE_INTEGER>(), Is.EqualTo(8));
        }
    }
}
