// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

using System;
using System.Threading;
using TerraFX.Interop;
using TerraFX.Utilities;
using static TerraFX.Graphics.Providers.D3D12.HelperUtilities;
using static TerraFX.Interop.D3D12_FENCE_FLAGS;
using static TerraFX.Interop.Windows;
using static TerraFX.Utilities.ExceptionUtilities;
using static TerraFX.Utilities.State;

namespace TerraFX.Graphics.Providers.D3D12
{
    /// <inheritdoc />
    public sealed unsafe class D3D12GraphicsFence : GraphicsFence
    {
        private ValueLazy<Pointer<ID3D12Fence>> _d3d12Fence;
        private ValueLazy<HANDLE> _d3d12FenceSignalEvent;

        private ulong _d3d12FenceSignalValue;

        private State _state;

        internal D3D12GraphicsFence(D3D12GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
        {
            _d3d12Fence = new ValueLazy<Pointer<ID3D12Fence>>(CreateD3D12Fence);
            _d3d12FenceSignalEvent = new ValueLazy<HANDLE>(CreateEventHandle);

            _ = _state.Transition(to: Initialized);
        }

        /// <summary>Finalizes an instance of the <see cref="D3D12GraphicsFence" /> class.</summary>
        ~D3D12GraphicsFence()
        {
            Dispose(isDisposing: false);
        }

        /// <summary>Gets the underlying <see cref="ID3D12Fence" /> for the fence.</summary>
        /// <exception cref="ObjectDisposedException">The fence has been disposed.</exception>
        public ID3D12Fence* D3D12Fence => _d3d12Fence.Value;

        /// <summary>Gets a <see cref="HANDLE" /> to an event which is raised when the fence enters the signalled state.</summary>
        /// <exception cref="ObjectDisposedException">The fence has been disposed.</exception>
        public HANDLE D3D12FenceSignalEvent => _d3d12FenceSignalEvent.Value;

        /// <summary>Gets the value at which the fence will enter the signalled state.</summary>
        /// <exception cref="ObjectDisposedException">The fence has been disposed.</exception>
        public ulong D3D12FenceSignalValue => _d3d12FenceSignalValue;

        /// <inheritdoc cref="GraphicsFence.GraphicsDevice" />
        public D3D12GraphicsDevice D3D12GraphicsDevice => (D3D12GraphicsDevice)GraphicsDevice;

        /// <inheritdoc />
        public override bool IsSignalled => D3D12Fence->GetCompletedValue() >= D3D12FenceSignalValue;

        /// <inheritdoc />
        public override void Reset()
        {
            if (IsSignalled)
            {
                _d3d12FenceSignalValue = D3D12Fence->GetCompletedValue() + 1;
            }
        }

        /// <inheritdoc />
        public override bool TryWait(int millisecondsTimeout = -1)
        {
            if (millisecondsTimeout < Timeout.Infinite)
            {
                ThrowArgumentOutOfRangeException(nameof(millisecondsTimeout), millisecondsTimeout);
            }
            return TryWait(unchecked((uint)millisecondsTimeout));
        }

        /// <inheritdoc />
        public override bool TryWait(TimeSpan timeout)
        {
            var remainingMilliseconds = (long)timeout.TotalMilliseconds;

            if (remainingMilliseconds < Timeout.Infinite)
            {
                ThrowArgumentOutOfRangeException(nameof(timeout), timeout);
            }

            var fenceSignalled = false;

            while (remainingMilliseconds > INFINITE)
            {
                const uint millisecondsTimeout = INFINITE - 1;

                if (TryWait(millisecondsTimeout))
                {
                    fenceSignalled = true;
                    break;
                }

                remainingMilliseconds -= millisecondsTimeout;
            }

            return fenceSignalled || TryWait(unchecked((uint)remainingMilliseconds));
        }

        /// <inheritdoc />
        protected override void Dispose(bool isDisposing)
        {
            var priorState = _state.BeginDispose();

            if (priorState < Disposing)
            {
                _d3d12Fence.Dispose(ReleaseIfNotNull);
                _d3d12FenceSignalEvent.Dispose(DisposeEventHandle);
            }

            _state.EndDispose();
        }

        private Pointer<ID3D12Fence> CreateD3D12Fence()
        {
            _state.AssertNotDisposedOrDisposing();

            ID3D12Fence* d3d12Fence;

            var iid = IID_ID3D12Fence;
            ThrowExternalExceptionIfFailed(nameof(ID3D12Device.CreateFence), D3D12GraphicsDevice.D3D12Device->CreateFence(InitialValue: 0, D3D12_FENCE_FLAG_NONE, &iid, (void**)&d3d12Fence));

            return d3d12Fence;
        }

        private HANDLE CreateEventHandle()
        {
            _state.AssertNotDisposedOrDisposing();

            HANDLE eventHandle = CreateEventW(lpEventAttributes: null, bManualReset: FALSE, bInitialState: FALSE, lpName: null);

            if (eventHandle == null)
            {
                ThrowExternalExceptionForLastHRESULT(nameof(CreateEventW));
            }

            return eventHandle;
        }

        private void DisposeEventHandle(HANDLE eventHandle)
        {
            _state.AssertDisposing();

            if (eventHandle != null)
            {
                _ = CloseHandle(eventHandle);
            }
        }

        private bool TryWait(uint millisecondsTimeout)
        {
            var fenceSignalled = IsSignalled;

            var fence = D3D12Fence;
            var fenceSignalEvent = D3D12FenceSignalEvent;

            if (!fenceSignalled)
            {
                ThrowExternalExceptionIfFailed(nameof(ID3D12Fence.SetEventOnCompletion), D3D12Fence->SetEventOnCompletion(D3D12FenceSignalValue, fenceSignalEvent));

                var result = WaitForSingleObject(fenceSignalEvent, millisecondsTimeout);

                if (result == WAIT_OBJECT_0)
                {
                    fenceSignalled = true;
                }
                else if (result != WAIT_TIMEOUT)
                {
                    ThrowExternalExceptionForLastError(nameof(WaitForSingleObject));
                }
            }

            return fenceSignalled;
        }
    }
}
