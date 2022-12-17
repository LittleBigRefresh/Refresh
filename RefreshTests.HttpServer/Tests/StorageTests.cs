using System.Text;
using Refresh.HttpServer.Storage;

namespace RefreshTests.HttpServer.Tests;

[NonParallelizable]
public class StorageTests
{
    private IDataStore _dataStore;
    private byte[] _value;
    
    [SetUp]
    public void SetUp()
    {
        this._dataStore = new InMemoryDataStore();
        this._value = Encoding.Default.GetBytes("value");
    }
    
    
    [Test]
    public void DataStoreSystemWorks()
    {
        Assert.False(this._dataStore.ExistsInStore("key"));
        Assert.That(() => this._dataStore.GetDataFromStore("key"), Throws.Exception);
        
        Assert.True(this._dataStore.WriteToStore("key", this._value));
        
        Assert.True(this._dataStore.ExistsInStore("key"));
        Assert.That(this._dataStore.GetDataFromStore("key"), Is.EqualTo(this._value));
    }
}