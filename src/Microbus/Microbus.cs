// <copyright file="Microbus.cs" company="Microbus contributors">
//  Copyright (c) Microbus contributors. All rights reserved.
// </copyright>
// <summary>A very lightweight in-memory message bus for .NET</summary>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;

[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1636:FileHeaderCopyrightTextMustMatch", Scope = "Module", Justification = "Content is valid.")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1641:FileHeaderCompanyNameTextMustMatch", Scope = "Module", Justification = "Content is valid.")]

#pragma warning disable 0436

// ReSharper disable CheckNamespace
// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PossibleNullReferenceException
// ReSharper disable RedundantNameQualifier
// ReSharper disable UnusedMember.Global

/// <summary>
/// <see cref="Microbus"/>. A very lightweight in-memory message bus for .NET.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "May not be used.")]
[ExcludeFromCodeCoverage]
internal sealed class Microbus
{
    private readonly Dictionary<Type, object> registeredHandlers = new Dictionary<Type, object>();
    private readonly Dictionary<Type, Action<object>> registeredInvokers = new Dictionary<Type, Action<object>>();

    private int depth;
    private bool cyclicLimitTriggered;

    public void Register<T>(Action<T> messageHandler)
    {
        if (messageHandler == null)
        {
            throw new ArgumentNullException("messageHandler");
        }

        object messageHandlers;
        if (!this.registeredHandlers.TryGetValue(typeof(T), out messageHandlers))
        {
            this.registeredHandlers.Add(typeof(T), messageHandlers = new HashSet<Action<T>>());
            this.registeredInvokers.Add(typeof(T), message => this.Send((T)message));
        }

        ((HashSet<Action<T>>)messageHandlers).Add(messageHandler);
    }

    public void Send(object message)
    {
        if (message == null)
        {
            throw new ArgumentNullException("message");
        }

        Action<object> messageInvoker;
        if (!this.registeredInvokers.TryGetValue(message.GetType(), out messageInvoker))
        {
            // no handlers
            return;
        }

        messageInvoker.Invoke(message);
    }

    [DebuggerStepThrough]
    public void Send<T>(T message)
    {
        if (this.cyclicLimitTriggered)
        {
            return;
        }

        if (message == null)
        {
            throw new ArgumentNullException("message");
        }

        object messageHandlers;
        if (!this.registeredHandlers.TryGetValue(message.GetType(), out messageHandlers))
        {
            // no handlers
            return;
        }

        if (++this.depth >= 50)
        {
            this.cyclicLimitTriggered = true;
            return;
        }

        foreach (var messageHandler in (HashSet<Action<T>>)messageHandlers)
        {
            messageHandler.Invoke(message);
        }

        if (--this.depth == 1 && this.cyclicLimitTriggered)
        {
            throw new InvalidOperationException("The operation has resulted in a cyclic call to this method.");
        }
    }
}