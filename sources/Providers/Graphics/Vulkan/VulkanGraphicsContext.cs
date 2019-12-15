// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

using System;
using TerraFX.Interop;
using TerraFX.Numerics;
using TerraFX.Utilities;
using static TerraFX.Graphics.Providers.Vulkan.HelperUtilities;
using static TerraFX.Interop.VkCommandPoolCreateFlagBits;
using static TerraFX.Interop.VkComponentSwizzle;
using static TerraFX.Interop.VkImageAspectFlagBits;
using static TerraFX.Interop.VkImageViewType;
using static TerraFX.Interop.VkStructureType;
using static TerraFX.Interop.VkSubpassContents;
using static TerraFX.Interop.Vulkan;
using static TerraFX.Utilities.DisposeUtilities;
using static TerraFX.Utilities.State;

namespace TerraFX.Graphics.Providers.Vulkan
{
    /// <inheritdoc />
    public sealed unsafe class VulkanGraphicsContext : GraphicsContext
    {
        private readonly VulkanGraphicsFence _graphicsFence;
        private readonly VulkanGraphicsFence _waitForExecuteCompletionGraphicsFence;

        private ValueLazy<VkCommandBuffer> _vulkanCommandBuffer;
        private ValueLazy<VkCommandPool> _vulkanCommandPool;
        private ValueLazy<VkFramebuffer> _vulkanFramebuffer;
        private ValueLazy<VkImageView> _vulkanSwapChainImageView;

        private State _state;

        internal VulkanGraphicsContext(VulkanGraphicsDevice graphicsDevice, int index)
            : base(graphicsDevice, index)
        {
            _graphicsFence = new VulkanGraphicsFence(graphicsDevice);
            _waitForExecuteCompletionGraphicsFence = new VulkanGraphicsFence(graphicsDevice);

            _vulkanCommandBuffer = new ValueLazy<VkCommandBuffer>(CreateVulkanCommandBuffer);
            _vulkanCommandPool = new ValueLazy<VkCommandPool>(CreateVulkanCommandPool);
            _vulkanFramebuffer = new ValueLazy<VkFramebuffer>(CreateVulkanFramebuffer);
            _vulkanSwapChainImageView = new ValueLazy<VkImageView>(CreateVulkanSwapChainImageView);

            _ = _state.Transition(to: Initialized);

            WaitForExecuteCompletionGraphicsFence.Reset();
        }

        /// <summary>Finalizes an instance of the <see cref="VulkanGraphicsContext" /> class.</summary>
        ~VulkanGraphicsContext()
        {
            Dispose(isDisposing: false);
        }

        /// <inheritdoc />
        public override GraphicsFence GraphicsFence => VulkanGraphicsFence;

        /// <summary>Gets the <see cref="VkCommandBuffer" /> used by the context.</summary>
        /// <exception cref="ObjectDisposedException">The context has been disposed.</exception>
        public VkCommandBuffer VulkanCommandBuffer => _vulkanCommandBuffer.Value;

        /// <summary>Gets the <see cref="VkCommandPool" /> used by the context.</summary>
        /// <exception cref="ObjectDisposedException">The context has been disposed.</exception>
        public VkCommandPool VulkanCommandPool => _vulkanCommandPool.Value;

        /// <summary>Gets the <see cref="VkFramebuffer"/> used by the context.</summary>
        /// <exception cref="ObjectDisposedException">The context has been disposed.</exception>
        public VkFramebuffer VulkanFramebuffer => _vulkanFramebuffer.Value;

        /// <inheritdoc cref="GraphicsContext.GraphicsDevice" />
        public VulkanGraphicsDevice VulkanGraphicsDevice => (VulkanGraphicsDevice)GraphicsDevice;

        /// <inheritdoc cref="GraphicsFence" />
        public VulkanGraphicsFence VulkanGraphicsFence => _graphicsFence;

        /// <summary>Gets the <see cref="VkImageView" /> used by the context.</summary>
        /// <exception cref="ObjectDisposedException">The context has been disposed.</exception>
        public VkImageView VulkanSwapChainImageView => _vulkanSwapChainImageView.Value;

        /// <summary>Gets a graphics fence that is used to wait for the context to finish execution.</summary>
        public VulkanGraphicsFence WaitForExecuteCompletionGraphicsFence => _waitForExecuteCompletionGraphicsFence;

        /// <inheritdoc />
        public override void BeginFrame(ColorRgba backgroundColor)
        {
            var graphicsFence = VulkanGraphicsFence;

            graphicsFence.Wait();
            graphicsFence.Reset();

            var commandBufferBeginInfo = new VkCommandBufferBeginInfo {
                sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO,
            };

            var commandBuffer = VulkanCommandBuffer;
            ThrowExternalExceptionIfNotSuccess(nameof(vkBeginCommandBuffer), vkBeginCommandBuffer(commandBuffer, &commandBufferBeginInfo));

            var clearValue = new VkClearValue();

            clearValue.color.float32[0] = backgroundColor.Red;
            clearValue.color.float32[1] = backgroundColor.Green;
            clearValue.color.float32[2] = backgroundColor.Blue;
            clearValue.color.float32[3] = backgroundColor.Alpha;

            var graphicsDevice = VulkanGraphicsDevice;
            var graphicsSurface = graphicsDevice.GraphicsSurface;

            var graphicsSurfaceWidth = graphicsSurface.Width;
            var graphicsSurfaceHeight = graphicsSurface.Height;

            var renderPassBeginInfo = new VkRenderPassBeginInfo {
                sType = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO,
                renderPass = graphicsDevice.VulkanRenderPass,
                framebuffer = VulkanFramebuffer,
                renderArea = new VkRect2D {
                    extent = new VkExtent2D {
                        width = (uint)graphicsSurface.Width,
                        height = (uint)graphicsSurface.Height,
                    },
                },
                clearValueCount = 1,
                pClearValues = &clearValue,
            };

            vkCmdBeginRenderPass(commandBuffer, &renderPassBeginInfo, VK_SUBPASS_CONTENTS_INLINE);

            var viewport = new VkViewport {
                width = graphicsSurface.Width,
                height = graphicsSurface.Height,
                minDepth = 0.0f,
                maxDepth = 1.0f,
            };
            vkCmdSetViewport(commandBuffer, firstViewport: 0, viewportCount: 1, &viewport);

            var scissorRect = new VkRect2D {
                extent = new VkExtent2D {
                    width = (uint)graphicsSurface.Width,
                    height = (uint)graphicsSurface.Height,
                },
            };
            vkCmdSetScissor(commandBuffer, firstScissor: 0, scissorCount: 1, &scissorRect);
        }

        /// <inheritdoc />
        public override void EndFrame()
        {
            var commandBuffer = VulkanCommandBuffer;
            vkCmdEndRenderPass(commandBuffer);

            ThrowExternalExceptionIfNotSuccess(nameof(vkEndCommandBuffer), vkEndCommandBuffer(commandBuffer));

            var submitInfo = new VkSubmitInfo {
                sType = VK_STRUCTURE_TYPE_SUBMIT_INFO,
                commandBufferCount = 1,
                pCommandBuffers = (IntPtr*)&commandBuffer,
            };

            var executeGraphicsFence = WaitForExecuteCompletionGraphicsFence;
            ThrowExternalExceptionIfNotSuccess(nameof(vkQueueSubmit), vkQueueSubmit(VulkanGraphicsDevice.VulkanCommandQueue, submitCount: 1, &submitInfo, executeGraphicsFence.VulkanFence));

            executeGraphicsFence.Wait();
            executeGraphicsFence.Reset();
        }

        /// <inheritdoc />
        protected override void Dispose(bool isDisposing)
        {
            var priorState = _state.BeginDispose();

            if (priorState < Disposing)
            {
                _vulkanCommandBuffer.Dispose(DisposeVulkanCommandBuffer);
                _vulkanCommandPool.Dispose(DisposeVulkanCommandPool);
                _vulkanFramebuffer.Dispose(DisposeVulkanFramebuffer);
                _vulkanSwapChainImageView.Dispose(DisposeVulkanSwapChainImageView);

                DisposeIfNotNull(_waitForExecuteCompletionGraphicsFence);
                DisposeIfNotNull(_graphicsFence);
            }

            _state.EndDispose();
        }

        internal void OnGraphicsSurfaceSizeChanged(object? sender, PropertyChangedEventArgs<Vector2> eventArgs)
        {
            if (_vulkanFramebuffer.IsCreated)
            {
                var vulkanFramebuffer = _vulkanFramebuffer.Value;

                if (vulkanFramebuffer != VK_NULL_HANDLE)
                {
                    vkDestroyFramebuffer(VulkanGraphicsDevice.VulkanDevice, vulkanFramebuffer, pAllocator: null);
                }

                _vulkanFramebuffer.Reset(CreateVulkanFramebuffer);
            }

            if (_vulkanSwapChainImageView.IsCreated)
            {
                var vulkanSwapChainImageView = _vulkanSwapChainImageView.Value;

                if (vulkanSwapChainImageView != VK_NULL_HANDLE)
                {
                    vkDestroyImageView(VulkanGraphicsDevice.VulkanDevice, vulkanSwapChainImageView, pAllocator: null);
                }

                _vulkanSwapChainImageView.Reset(CreateVulkanSwapChainImageView);
            }
        }

        private VkCommandBuffer CreateVulkanCommandBuffer()
        {
            VkCommandBuffer vulkanCommandBuffer;

            var commandBufferAllocateInfo = new VkCommandBufferAllocateInfo {
                sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO,
                commandPool = VulkanCommandPool,
                commandBufferCount = 1,
            };
            ThrowExternalExceptionIfNotSuccess(nameof(vkAllocateCommandBuffers), vkAllocateCommandBuffers(VulkanGraphicsDevice.VulkanDevice, &commandBufferAllocateInfo, (IntPtr*)&vulkanCommandBuffer));

            return vulkanCommandBuffer;
        }

        private VkCommandPool CreateVulkanCommandPool()
        {
            VkCommandPool vulkanCommandPool;

            var commandPoolCreateInfo = new VkCommandPoolCreateInfo {
                sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO,
                flags = (uint)VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT,
                queueFamilyIndex = VulkanGraphicsDevice.VulkanCommandQueueFamilyIndex,
            };
            ThrowExternalExceptionIfNotSuccess(nameof(vkCreateCommandPool), vkCreateCommandPool(VulkanGraphicsDevice.VulkanDevice, &commandPoolCreateInfo, pAllocator: null, (ulong*)&vulkanCommandPool));

            return vulkanCommandPool;
        }

        private VkFramebuffer CreateVulkanFramebuffer()
        {
            VkFramebuffer vulkanFramebuffer;

            var graphicsDevice = VulkanGraphicsDevice;
            var graphicsSurface = graphicsDevice.GraphicsSurface;
            var swapChainImageView = VulkanSwapChainImageView;

            var frameBufferCreateInfo = new VkFramebufferCreateInfo {
                sType = VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO,
                renderPass = graphicsDevice.VulkanRenderPass,
                attachmentCount = 1,
                pAttachments = (ulong*)&swapChainImageView,
                width = (uint)graphicsSurface.Width,
                height = (uint)graphicsSurface.Height,
                layers = 1,
            };
            ThrowExternalExceptionIfNotSuccess(nameof(vkCreateFramebuffer), vkCreateFramebuffer(graphicsDevice.VulkanDevice, &frameBufferCreateInfo, pAllocator: null, (ulong*)&vulkanFramebuffer));

            return vulkanFramebuffer;
        }

        private VkImageView CreateVulkanSwapChainImageView()
        {
            VkImageView swapChainImageView;

            var graphicsDevice = VulkanGraphicsDevice;

            var swapChainImageViewCreateInfo = new VkImageViewCreateInfo {
                sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO,
                image = graphicsDevice.VulkanSwapchainImages[Index],
                viewType = VK_IMAGE_VIEW_TYPE_2D,
                format = graphicsDevice.VulkanSwapchainFormat,
                components = new VkComponentMapping {
                    r = VK_COMPONENT_SWIZZLE_R,
                    g = VK_COMPONENT_SWIZZLE_G,
                    b = VK_COMPONENT_SWIZZLE_B,
                    a = VK_COMPONENT_SWIZZLE_A,
                },
                subresourceRange = new VkImageSubresourceRange {
                    aspectMask = (uint)VK_IMAGE_ASPECT_COLOR_BIT,
                    levelCount = 1,
                    layerCount = 1,
                },
            };
            ThrowExternalExceptionIfNotSuccess(nameof(vkCreateImageView), vkCreateImageView(graphicsDevice.VulkanDevice, &swapChainImageViewCreateInfo, pAllocator: null, (ulong*)&swapChainImageView));

            return swapChainImageView;
        }

        private void DisposeVulkanCommandBuffer(VkCommandBuffer vulkanCommandBuffer)
        {
            _state.AssertDisposing();

            if (vulkanCommandBuffer != null)
            {
                vkFreeCommandBuffers(VulkanGraphicsDevice.VulkanDevice, VulkanCommandPool, 1, (IntPtr*)&vulkanCommandBuffer);
            }
        }

        private void DisposeVulkanCommandPool(VkCommandPool vulkanCommandPool)
        {
            _state.AssertDisposing();

            if (vulkanCommandPool != VK_NULL_HANDLE)
            {
                vkDestroyCommandPool(VulkanGraphicsDevice.VulkanDevice, vulkanCommandPool, pAllocator: null);
            }
        }

        private void DisposeVulkanFramebuffer(VkFramebuffer vulkanFramebuffer)
        {
            _state.AssertDisposing();

            if (vulkanFramebuffer != VK_NULL_HANDLE)
            {
                vkDestroyFramebuffer(VulkanGraphicsDevice.VulkanDevice, vulkanFramebuffer, pAllocator: null);
            }
        }

        private void DisposeVulkanSwapChainImageView(VkImageView vulkanSwapchainImageView)
        {
            _state.AssertDisposing();

            if (vulkanSwapchainImageView != VK_NULL_HANDLE)
            {
                vkDestroyImageView(VulkanGraphicsDevice.VulkanDevice, vulkanSwapchainImageView, pAllocator: null);
            }
        }
    }
}