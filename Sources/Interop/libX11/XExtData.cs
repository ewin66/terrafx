// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from X11\xlib.h in the Xlib - C Language X Interface: X Version 11, Release 7.7
// Original source is Copyright © The Open Group.

using System;
using System.Runtime.InteropServices;

namespace TerraFX.Interop
{
    public /* blittable */ unsafe struct XExtData
    {
        #region Fields
        public int number;

        public XExtData* next;

        [ComAliasName("free_private")]
        public IntPtr free_private;

        [ComAliasName("XPointer")]
        public sbyte* private_data;
        #endregion
    }
}
