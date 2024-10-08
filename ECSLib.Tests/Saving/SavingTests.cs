﻿using ECSLib.Binary;
using ECSLib.Components;
using ECSLib.Components.Interning;
using ECSLib.Entities;

namespace ECSLib.Tests.Saving;

public class SavingTests
{
    private const string SaveFilePath = "TestSave.data";

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(SaveFilePath))
            File.Delete(SaveFilePath);
    }
    
    [Test]
    public void SavingTest()
    {
        //Serialize
        ECS before = new();
        FillWorld(before);
        AssertWorld(before);
        using (var stream = File.Open(SaveFilePath, FileMode.Create))
        {
            using (BinaryWriter writer = new(stream))
            {
                Assert.DoesNotThrow(() => ECSSerializer.WriteWorldToBytes(before, writer));
                Console.WriteLine($"Save Data Size: {stream.Length}");
            }
        }
        before.Clear();
        
        //Deserialize
        ECS after = new();
        using (var stream = File.OpenRead(SaveFilePath))
            using (BinaryReader reader = new(stream))
                Assert.DoesNotThrow(() => ECSSerializer.ReadWorldFromBytes(after, reader));
        AssertWorld(after);
        after.Clear();
    }

    private static void FillWorld(ECS world)
    {
        var a = world.CreateEntity();
        RefPoolContext.BeginContext(a, world);
        world.AddComponent<CompA>(a, new(){Value = -1});
        RefPoolContext.EndContext(a, world);

        var b = world.CreateEntity();
        RefPoolContext.BeginContext(b, world);
        world.AddComponent<CompB>(b);
        world.GetComponent<CompB>(b).Text.Value = "...";
        RefPoolContext.EndContext(b, world);

        var c = world.CreateEntity();
        RefPoolContext.BeginContext(c, world);
        world.AddComponent<CompA>(c);
        world.GetComponent<CompA>(c).Classes.Value.Add(new(){Prop = 100});
        world.AddComponent<CompB>(c);
        RefPoolContext.EndContext(c, world);
    }

    private static void AssertWorld(ECS world)
    {
        world.Query(Query.With<CompA>().WithNone<CompB>(), (Entity e, ref Comp<CompA> a) =>
        {
            Assert.That(a.Value.Classes.Value, Is.Empty);
            Assert.That(a.Value.Value, Is.EqualTo(-1));
        });
        
        world.Query(Query.With<CompB>().WithNone<CompA>(), (Entity e, ref Comp<CompB> b) =>
        {
            Assert.That(b.Value.Text.Value, Is.EqualTo("..."));
        });
        
        world.Query(Query.With<CompA, CompB>(), (Entity e, ref Comp<CompA> a, ref Comp<CompB> b) =>
        {
            Assert.That(a.Value.Value, Is.EqualTo(10));
            Assert.That(a.Value.Classes.Value[0].Prop, Is.EqualTo(100));
            Assert.That(b.Value.Text.Value, Is.EqualTo("Yo"));
        });
    }
}