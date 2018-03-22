// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um\wincodec.h in the Windows SDK for Windows 10.0.15063.0
// Original source is Copyright © Microsoft. All rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Security;
using static TerraFX.Utilities.InteropUtilities;

namespace TerraFX.Interop
{
    [Guid("2F0C601F-D2C6-468C-ABFA-49495D983ED1")]
    public /* blittable */ unsafe struct IWICJpegFrameEncode
    {
        #region Fields
        public readonly Vtbl* lpVtbl;
        #endregion

        #region IUnknown Delegates
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = false, ThrowOnUnmappableChar = false)]
        [return: ComAliasName("HRESULT")]
        public /* static */ delegate int _QueryInterface(
            [In] IWICJpegFrameEncode* This,
            [In, ComAliasName("REFIID")] Guid* riid,
            [Out] void** ppvObject
        );

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = false, ThrowOnUnmappableChar = false)]
        [return: ComAliasName("ULONG")]
        public /* static */ delegate uint _AddRef(
            [In] IWICJpegFrameEncode* This
        );

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = false, ThrowOnUnmappableChar = false)]
        [return: ComAliasName("ULONG")]
        public /* static */ delegate uint _Release(
            [In] IWICJpegFrameEncode* This
        );
        #endregion

        #region Delegates
        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = false, ThrowOnUnmappableChar = false)]
        [return: ComAliasName("HRESULT")]
        public /* static */ delegate int _GetAcHuffmanTable(
            [In] IWICJpegFrameEncode* This,
            [In, ComAliasName("UINT")] uint scanIndex,
            [In, ComAliasName("UINT")] uint tableIndex,
            [Out] DXGI_JPEG_AC_HUFFMAN_TABLE* pAcHuffmanTable
        );

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = false, ThrowOnUnmappableChar = false)]
        [return: ComAliasName("HRESULT")]
        public /* static */ delegate int _GetDcHuffmanTable(
            [In] IWICJpegFrameEncode* This,
            [In, ComAliasName("UINT")] uint scanIndex,
            [In, ComAliasName("UINT")] uint tableIndex,
            [Out] DXGI_JPEG_DC_HUFFMAN_TABLE* pDcHuffmanTable
        );

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = false, ThrowOnUnmappableChar = false)]
        [return: ComAliasName("HRESULT")]
        public /* static */ delegate int _GetQuantizationTable(
            [In] IWICJpegFrameEncode* This,
            [In, ComAliasName("UINT")] uint scanIndex,
            [In, ComAliasName("UINT")] uint tableIndex,
            [Out] DXGI_JPEG_QUANTIZATION_TABLE* pQuantizationTable
        );

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = false, ThrowOnUnmappableChar = false)]
        [return: ComAliasName("HRESULT")]
        public /* static */ delegate int _WriteScan(
            [In] IWICJpegFrameEncode* This,
            [In, ComAliasName("UINT")] uint cbScanData,
            [In, ComAliasName("BYTE[]")] byte* pbScanData
        );
        #endregion

        #region IUnknown Methods
        [return: ComAliasName("HRESULT")]
        public int QueryInterface(
            [In, ComAliasName("REFIID")] Guid* riid,
            [Out] void** ppvObject
        )
        {
            fixed (IWICJpegFrameEncode* This = &this)
            {
                return MarshalFunction<_QueryInterface>(lpVtbl->QueryInterface)(
                    This,
                    riid,
                    ppvObject
                );
            }
        }

        [return: ComAliasName("ULONG")]
        public uint AddRef()
        {
            fixed (IWICJpegFrameEncode* This = &this)
            {
                return MarshalFunction<_AddRef>(lpVtbl->AddRef)(
                    This
                );
            }
        }

        [return: ComAliasName("ULONG")]
        public uint Release()
        {
            fixed (IWICJpegFrameEncode* This = &this)
            {
                return MarshalFunction<_Release>(lpVtbl->Release)(
                    This
                );
            }
        }
        #endregion

        #region Methods
        [return: ComAliasName("HRESULT")]
        public int GetAcHuffmanTable(
            [In, ComAliasName("UINT")] uint scanIndex,
            [In, ComAliasName("UINT")] uint tableIndex,
            [Out] DXGI_JPEG_AC_HUFFMAN_TABLE* pAcHuffmanTable
        )
        {
            fixed (IWICJpegFrameEncode* This = &this)
            {
                return MarshalFunction<_GetAcHuffmanTable>(lpVtbl->GetAcHuffmanTable)(
                    This,
                    scanIndex,
                    tableIndex,
                    pAcHuffmanTable
                );
            }
        }

        [return: ComAliasName("HRESULT")]
        public int GetDcHuffmanTable(
            [In, ComAliasName("UINT")] uint scanIndex,
            [In, ComAliasName("UINT")] uint tableIndex,
            [Out] DXGI_JPEG_DC_HUFFMAN_TABLE* pDcHuffmanTable
        )
        {
            fixed (IWICJpegFrameEncode* This = &this)
            {
                return MarshalFunction<_GetDcHuffmanTable>(lpVtbl->GetDcHuffmanTable)(
                    This,
                    scanIndex,
                    tableIndex,
                    pDcHuffmanTable
                );
            }
        }

        [return: ComAliasName("HRESULT")]
        public int GetQuantizationTable(
            [In, ComAliasName("UINT")] uint scanIndex,
            [In, ComAliasName("UINT")] uint tableIndex,
            [Out] DXGI_JPEG_QUANTIZATION_TABLE* pQuantizationTable
        )
        {
            fixed (IWICJpegFrameEncode* This = &this)
            {
                return MarshalFunction<_GetQuantizationTable>(lpVtbl->GetQuantizationTable)(
                    This,
                    scanIndex,
                    tableIndex,
                    pQuantizationTable
                );
            }
        }

        [return: ComAliasName("HRESULT")]
        public int WriteScan(
            [In, ComAliasName("UINT")] uint cbScanData,
            [In, ComAliasName("BYTE[]")] byte* pbScanData
        )
        {
            fixed (IWICJpegFrameEncode* This = &this)
            {
                return MarshalFunction<_WriteScan>(lpVtbl->WriteScan)(
                    This,
                    cbScanData,
                    pbScanData
                );
            }
        }
        #endregion

        #region Structs
        public /* blittable */ struct Vtbl
        {
            #region IUnknown Fields
            public IntPtr QueryInterface;

            public IntPtr AddRef;

            public IntPtr Release;
            #endregion

            #region Fields
            public IntPtr GetAcHuffmanTable;

            public IntPtr GetDcHuffmanTable;

            public IntPtr GetQuantizationTable;

            public IntPtr WriteScan;
            #endregion
        }
        #endregion
    }
}
