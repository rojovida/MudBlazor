﻿// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

class MudElementReference {
    constructor() {
        this.listenerId = 0;
        this.eventListeners = {};
    }

    focus (element) {
        if (element)
        {
            element.focus();
        }
    }

    blur(element) {
        if (element) {
            element.blur();
        }
    }

    focusFirst (element, skip = 0, min = 0) {
        if (element)
        {
            let tabbables = getTabbableElements(element);
            if (tabbables.length <= min)
                element.focus();
            else
                tabbables[skip].focus();
        }
    }

    focusLast (element, skip = 0, min = 0) {
        if (element)
        {
            let tabbables = getTabbableElements(element);
            if (tabbables.length <= min)
                element.focus();
            else
                tabbables[tabbables.length - skip - 1].focus();
        }
    }

    saveFocus (element) {
        if (element)
        {
            element['mudblazor_savedFocus'] = document.activeElement;
        }
    }

    restoreFocus (element) {
        if (element)
        {
            let previous = element['mudblazor_savedFocus'];
            delete element['mudblazor_savedFocus']
            if (previous)
                previous.focus();
        }
    }

    selectRange(element, pos1, pos2) {
        if (element)
        {
            if (element.createTextRange) {
                let selRange = element.createTextRange();
                selRange.collapse(true);
                selRange.moveStart('character', pos1);
                selRange.moveEnd('character', pos2);
                selRange.select();
            } else if (element.setSelectionRange) {
                element.setSelectionRange(pos1, pos2);
            } else if (element.selectionStart) {
                element.selectionStart = pos1;
                element.selectionEnd = pos2;
            }
            element.focus();
        }
    }

    select(element) {
        if (element)
        {
            element.select();
        }
    }

    getBoundingClientRect(element) {
        if (!element) return;

        var rect = JSON.parse(JSON.stringify(element.getBoundingClientRect()));

        rect.scrollY = window.scrollY || document.documentElement.scrollTop;
        rect.scrollX = window.scrollX || document.documentElement.scrollLeft;

        rect.windowHeight = window.innerHeight;
        rect.windowWidth = window.innerWidth;
        return rect;
    }

    changeCss (element, css) {
        if (element)
        {
            element.className = css;
        }
    }

    removeEventListener (element, event, eventId) {
        element.removeEventListener(event, this.eventListeners[eventId]);
        delete this.eventListeners[eventId];
    }

    addDefaultPreventingHandler(element, eventName) {
        let listener = function (e) {
            // Only prevent default if not already prevented
            if (!e.defaultPrevented) {
                e.preventDefault();
            }
        };

        element.addEventListener(eventName, listener, { passive: false });
        this.eventListeners[++this.listenerId] = listener;
        return this.listenerId;
    }

    removeDefaultPreventingHandler(element, eventName, listenerId) {
        this.removeEventListener(element, eventName, listenerId);
    }

    addDefaultPreventingHandlers(element, eventNames) {
        let listeners = [];

        for (const eventName of eventNames) {
            let listenerId = this.addDefaultPreventingHandler(element, eventName);
            listeners.push(listenerId);
        }

        return listeners;
    }

    removeDefaultPreventingHandlers(element, eventNames, listenerIds) {
        for (let index = 0; index < eventNames.length; ++index) {
            const eventName = eventNames[index];
            const listenerId = listenerIds[index];
            this.removeDefaultPreventingHandler(element, eventName, listenerId);
        }
    }

    // ios doesn't trigger Blazor/React/Other dom style blur event so add a base event listener here 
    // that will trigger with IOS Done button and regular blur events
    addOnBlurEvent(element, dotNetReference) {
        if (!element) return;

        element._mudBlurHandler = function (e) {
            if (!element || !document.contains(element)) {
                // Element is no longer in the DOM, clean up
                window.mudElementRef.removeOnBlurEvent(element);
                return;
            }
            e.preventDefault();
            
            if (dotNetReference) {
                dotNetReference.invokeMethodAsync('CallOnBlurredAsync').catch(err => {
                    console.warn("Error invoking CallOnBlurredAsync, possibly disposed:", err);
                    window.mudElementRef.removeOnBlurEvent(element);
                });
            } else {
                console.error("No dotNetReference found for iosKeyboardFocus");
            }
        };

        element.addEventListener('blur', element._mudBlurHandler);
    }

    removeOnBlurEvent(element) {
        if (!element) return;
        if (element._mudBlurHandler) {
            element.removeEventListener('blur', element._mudBlurHandler);
            delete element._mudBlurHandler;
        }
    }
};
window.mudElementRef = new MudElementReference();
