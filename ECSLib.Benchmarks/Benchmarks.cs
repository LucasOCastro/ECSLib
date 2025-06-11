using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using ECSLib.Benchmarks.ECSImplementation;
using ECSLib.Benchmarks.OOPImplementation;

namespace ECSLib.Benchmarks;

[MemoryDiagnoser]
public class Benchmarks
{
    private const int EntityCount = 100000;
    private const float WithVelocityRatio = 0.5f;
    private const float WithVelocityBoundRatio = 0.15f;
        
    private const int WithVelocityCount = (int)(EntityCount * WithVelocityRatio);
    private static readonly Slice WithVelocitySlice = new(0, WithVelocityCount);
        
    private const int WithVelocityBoundCount = (int)(EntityCount * WithVelocityBoundRatio);
    private static readonly Slice WithVelocityBoundSlice = new(WithVelocitySlice.End, WithVelocityBoundCount);

    private ECS _ecsWorld;
    private List<OOPEntity> _oopEntities;
    private List<OOPEntity> _shuffledOopEntities;
    
    [GlobalSetup]
    public void Setup()
    {
        _ecsWorld = new();
        _ecsWorld.RegisterSystem<VelocitySystem>();
        var ecsFactory = new ECSEntityFactory(_ecsWorld);
            
        _oopEntities = [];
        _shuffledOopEntities = [];
        var oopFactory = new OOPEntityFactory();
        
        var rng = new Random(42);
        var shuffledIndices = Enumerable.Range(0, EntityCount).ToArray();
        rng.Shuffle(shuffledIndices);
        
        for (int orderedIndex = 0; orderedIndex < EntityCount; orderedIndex++)
        {
            int shuffledIndex = shuffledIndices[orderedIndex];
            
            // Inserts ECS entities shuffled to guarantee internal sorting
            MakeEntity(ecsFactory, shuffledIndex);
            
            // Inserts OOP entities
            _oopEntities.Add(MakeEntity(oopFactory, orderedIndex));
            _shuffledOopEntities.Add(MakeEntity(oopFactory, shuffledIndex));
        }
    }
    
    [Benchmark]
    public void ECSSystemBenchmark()
    {
        _ecsWorld.ProcessSystems();
    }

    [Benchmark]
    public void OOPEntitiesBenchmark()
    {
        foreach (var entity in _oopEntities)
            entity.Update();
    }
    
    [Benchmark]
    public void ShuffledOOPEntitiesBenchmark()
    {
        foreach (var entity in _shuffledOopEntities)
            entity.Update();
    }
        
    private static T MakeEntity<T>(IEntityFactory<T> factory, int i)
    {
        factory.WithHealth(i).WithPosition(i, i);
        if (WithVelocitySlice.Contains(i) || WithVelocityBoundSlice.Contains(i)) 
            factory.WithVelocity(i % 9 - 4, i % 5 - 3);
        if (WithVelocityBoundSlice.Contains(i)) 
            factory.WithBoundingBox(-(i + 1), -(i + 1), 2 * (i + 1), 2 * (i + 1));
        return factory.Build();
    }
}