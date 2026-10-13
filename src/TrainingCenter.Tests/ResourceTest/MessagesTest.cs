namespace TrainingCenter.Tests.ResourceTest;

using System.Globalization;
using System.Resources;
using TrainingCenter.Core.Resources;
using Xunit;

/// <summary>
/// Тест ресурсов <see cref="Messages"/>.
/// </summary>
public class MessagesTest
{
    /// <summary>
    /// Проверяет наличие непустого перевода каждого ключа в обеих культурах.
    /// </summary>
    [Fact]
    public void ResourceTranslationsTest()
    {
        var manager = new ResourceManager("TrainingCenter.Core.Resources.Messages", typeof(Messages).Assembly);
        var english = manager.GetResourceSet(CultureInfo.GetCultureInfo("en"), true, true)!;
        var russian = manager.GetResourceSet(CultureInfo.GetCultureInfo("ru"), true, false)!;
        var englishKeys = english.Cast<System.Collections.DictionaryEntry>().Select(entry => (string)entry.Key).Order().ToArray();
        var russianKeys = russian.Cast<System.Collections.DictionaryEntry>().Select(entry => (string)entry.Key).Order().ToArray();

        Assert.NotEmpty(englishKeys);
        Assert.Equal(englishKeys, russianKeys);
        foreach (var key in englishKeys)
        {
            Assert.False(string.IsNullOrWhiteSpace(english.GetString(key)));
            Assert.False(string.IsNullOrWhiteSpace(russian.GetString(key)));
        }
    }
}