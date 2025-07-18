﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor.Interop;

namespace MudBlazor
{
#nullable enable
    [ExcludeFromCodeCoverage]
    public static class ElementReferenceExtensions
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<JSRuntime>k__BackingField")]
        private static extern ref IJSRuntime GetJsRuntime(WebElementReferenceContext context);

        internal static IJSRuntime? GetJSRuntime(this ElementReference elementReference)
        {
            if (elementReference.Context is WebElementReferenceContext context)
            {
                var jsRuntime = GetJsRuntime(context);

                return jsRuntime;
            }

            return null;
        }

        public static ValueTask MudFocusFirstAsync(this ElementReference elementReference, int skip = 0, int min = 0) =>
            elementReference.GetJSRuntime()?.InvokeVoidAsync("mudElementRef.focusFirst", elementReference, skip, min) ?? ValueTask.CompletedTask;

        public static ValueTask MudFocusLastAsync(this ElementReference elementReference, int skip = 0, int min = 0) =>
            elementReference.GetJSRuntime()?.InvokeVoidAsync("mudElementRef.focusLast", elementReference, skip, min) ?? ValueTask.CompletedTask;

        public static ValueTask MudSaveFocusAsync(this ElementReference elementReference) =>
            elementReference.GetJSRuntime()?.InvokeVoidAsync("mudElementRef.saveFocus", elementReference) ?? ValueTask.CompletedTask;

        public static ValueTask MudRestoreFocusAsync(this ElementReference elementReference) =>
            elementReference.GetJSRuntime()?.InvokeVoidAsync("mudElementRef.restoreFocus", elementReference) ?? ValueTask.CompletedTask;

        public static ValueTask MudBlurAsync(this ElementReference elementReference) =>
            elementReference.GetJSRuntime()?.InvokeVoidAsync("mudElementRef.blur", elementReference) ?? ValueTask.CompletedTask;

        public static ValueTask MudSelectAsync(this ElementReference elementReference) =>
            elementReference.GetJSRuntime()?.InvokeVoidAsync("mudElementRef.select", elementReference) ?? ValueTask.CompletedTask;

        public static ValueTask MudSelectRangeAsync(this ElementReference elementReference, int pos1, int pos2) =>
            elementReference.GetJSRuntime()?.InvokeVoidAsync("mudElementRef.selectRange", elementReference, pos1, pos2) ?? ValueTask.CompletedTask;

        public static ValueTask MudChangeCssAsync(this ElementReference elementReference, string css) =>
            elementReference.GetJSRuntime()?.InvokeVoidAsync("mudElementRef.changeCss", elementReference, css) ?? ValueTask.CompletedTask;

        public static ValueTask<BoundingClientRect> MudGetBoundingClientRectAsync(this ElementReference elementReference) =>
            elementReference.GetJSRuntime()?.InvokeAsync<BoundingClientRect>("mudElementRef.getBoundingClientRect", elementReference) ?? ValueTask.FromResult(new BoundingClientRect());

        public static ValueTask<int[]> AddDefaultPreventingHandlers(this ElementReference elementReference, string[] eventNames) =>
            elementReference.GetJSRuntime()?.InvokeAsync<int[]>("mudElementRef.addDefaultPreventingHandlers", elementReference, eventNames) ?? new ValueTask<int[]>(Array.Empty<int>());

        public static ValueTask RemoveDefaultPreventingHandlers(this ElementReference elementReference, string[] eventNames, int[] listenerIds)
        {
            if (eventNames.Length != listenerIds.Length)
            {
                throw new ArgumentException($"Number of elements in {nameof(eventNames)} and {nameof(listenerIds)} has to match.");
            }

            return elementReference.GetJSRuntime()?.InvokeVoidAsync("mudElementRef.removeDefaultPreventingHandlers", elementReference, eventNames, listenerIds) ?? ValueTask.CompletedTask;
        }

        public static ValueTask MudAttachBlurEventWithJS<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(
            this ElementReference elementReference,
            DotNetObjectReference<T> obj) where T : class =>
            elementReference.GetJSRuntime()?.InvokeVoidAsync("mudElementRef.addOnBlurEvent", elementReference, obj) ?? ValueTask.CompletedTask;

        [Obsolete("Use mudElementRef.removeOnBlurEvent via js invoke instead")]
        public static ValueTask MudDetachBlurEventWithJS<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(
            this ElementReference elementReference,
            DotNetObjectReference<T> obj) where T : class =>
            elementReference.GetJSRuntime()?.InvokeVoidAsync("mudElementRef.removeOnBlurEvent", elementReference, obj) ?? ValueTask.CompletedTask;
    }
}
