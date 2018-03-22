// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from shared\dxgi.h in the Windows SDK for Windows 10.0.15063.0
// Original source is Copyright © Microsoft. All rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace TerraFX.Interop.Desktop
{
    public static unsafe partial class DXGI
    {
        private const string DllName = nameof(DXGI);

        #region External Methods
        [DllImport(DllName, BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "CreateDXGIFactory", ExactSpelling = true, PreserveSig = true, SetLastError = false, ThrowOnUnmappableChar = false)]
        [SuppressUnmanagedCodeSecurity]
        [return: ComAliasName("HRESULT")]
        public static extern int CreateDXGIFactory(
            [In, ComAliasName("REFIID")] Guid* riid,
            [Out] void** ppFactory
        );
        #endregion
    }
}