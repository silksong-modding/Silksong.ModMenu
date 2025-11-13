namespace Silksong.ModMenu.Models;

/// <summary>
/// A selector of one or more values that can be navigated with left and right inputs, and displayed as text.
/// </summary>
/// <typeparam name="T">The type of value represented.</typeparam>
public interface IChoiceModel<T> : IValueModel<T>, IBaseChoiceModel { }
