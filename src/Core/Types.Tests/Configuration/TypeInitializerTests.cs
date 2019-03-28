using System.Collections.Generic;
using System.Linq;
using HotChocolate.Configuration;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using HotChocolate.Utilities;
using Moq;
using Snapshooter;
using Snapshooter.Xunit;
using Xunit;

namespace HotChocolate
{
    public class TypeInitializerTests
    {
        [Fact]
        public void Register_SchemaType_ClrTypeExists()
        {
            // arrange
            var initialTypes = new List<ITypeReference>();
            initialTypes.Add(new ClrTypeReference(
                typeof(FooType),
                TypeContext.Output));

            var serviceProvider = new EmptyServiceProvider();

            var typeInitializer = new TypeInitializer(
                serviceProvider,
                initialTypes,
                t => t is FooType);

            // act
            typeInitializer.Initialize(() => null);

            // assert
            bool exists = typeInitializer.Types.TryGetValue(
                new ClrTypeReference(typeof(FooType), TypeContext.Output),
                out RegisteredType type);

            Assert.True(exists);
            Assert.IsType<FooType>(type.Type).Fields.ToDictionary(
                t => t.Name.ToString(),
                t => TypeVisualizer.Visualize(t.Type))
                .MatchSnapshot(new SnapshotNameExtension("FooType"));

            exists = typeInitializer.Types.TryGetValue(
                new ClrTypeReference(typeof(BarType), TypeContext.Output),
                out type);

            Assert.True(exists);
            Assert.IsType<BarType>(type.Type).Fields.ToDictionary(
                t => t.Name.ToString(),
                t => TypeVisualizer.Visualize(t.Type))
                .MatchSnapshot(new SnapshotNameExtension("BarType"));
        }

        [Fact]
        public void Register_ClrType_InferSchemaTypes()
        {
            // arrange
            var initialTypes = new List<ITypeReference>();
            initialTypes.Add(new ClrTypeReference(
                typeof(Foo),
                TypeContext.Output));

            var serviceProvider = new EmptyServiceProvider();

            var typeInitializer = new TypeInitializer(
                serviceProvider,
                initialTypes,
                t => t is ObjectType<Foo>);

            // act
            typeInitializer.Initialize(() => null);

            // assert
            bool exists = typeInitializer.Types.TryGetValue(
                new ClrTypeReference(
                    typeof(ObjectType<Foo>),
                    TypeContext.Output),
                out RegisteredType type);

            Assert.True(exists);
            Assert.IsType<ObjectType<Foo>>(type.Type).Fields.ToDictionary(
                t => t.Name.ToString(),
                t => TypeVisualizer.Visualize(t.Type))
                .MatchSnapshot(new SnapshotNameExtension("FooType"));

            exists = typeInitializer.Types.TryGetValue(
                new ClrTypeReference(typeof(ObjectType<Bar>), TypeContext.Output),
                out type);

            Assert.True(exists);
            Assert.IsType<ObjectType<Bar>>(type.Type).Fields.ToDictionary(
                t => t.Name.ToString(),
                t => TypeVisualizer.Visualize(t.Type))
                .MatchSnapshot(new SnapshotNameExtension("BarType"));
        }


        public class FooType
            : ObjectType<Foo>
        {
            protected override void Configure(
                IObjectTypeDescriptor<Foo> descriptor)
            {
                descriptor.Field(t => t.Bar).Type<NonNullType<BarType>>();
            }
        }

        public class BarType
            : ObjectType<Bar>
        {
        }

        public class Foo
        {
            public Bar Bar { get; }
        }

        public class Bar
        {
            public string Baz { get; }
        }
    }
}
