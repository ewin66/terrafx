// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace TerraFX.Interop.UnitTests
{
    /// <summary>Provides validation of the <see cref="D2D1_SVG_LENGTH" /> struct.</summary>
    public static class D2D1_SVG_LENGTHTests
    {
        /// <summary>Validates that the layout of the <see cref="D2D1_SVG_LENGTH" /> struct is <see cref="LayoutKind.Sequential" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(D2D1_SVG_LENGTH).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the size of the <see cref="D2D1_SVG_LENGTH" /> struct is correct.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(Marshal.SizeOf<D2D1_SVG_LENGTH>(), Is.EqualTo(8));
        }
    }
}