using System;
using Should;
using Xunit;

namespace ExpressionActivator.Tests
{
    public class ExpressionActivatorTests
    {
        [Fact]
        public void CanCreateInstanceFromDefaultConstructor()
        {
            var activator = typeof(Person).CreateActivator();

            var instance = activator.Invoke();

            instance.ShouldNotBeNull();
        }

        [Fact]
        public void CanCreateInstanceWithParameters()
        {
            var activator = typeof(Person).CreateActivator(typeof(string), typeof(string));

            var instance = (Person) activator.Invoke("Kristian", "Hellang");

            instance.ShouldNotBeNull();
            instance.FirstName.ShouldEqual("Kristian");
            instance.LastName.ShouldEqual("Hellang");
        }

        [Fact]
        public void ShouldThrowIfConstructorIsNotFound()
        {
            Assert.Throws<ConstructorNotFoundException>(() => typeof(Person).CreateActivator(typeof(string)));
            Assert.Throws<ConstructorNotFoundException>(() => ExpressionActivator.CreateActivator<Person>(typeof(string)));
        }

        [Fact]
        public void ShouldThrowIfConstructorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => typeof(Person).GetConstructor(new[] { typeof(string) }).CreateActivator());
        }

        [Fact]
        public void ShouldThrowTypeMismatchExceptionIfTypesDoesntMatch()
        {
            var constructor = typeof(Person).GetConstructor(Type.EmptyTypes);

            Assert.Throws<TypeMismatchException>(() => constructor.CreateActivator<string>());
        }

        [Fact]
        public void CanCreateTypedInstanceFromDefaultConstructor()
        {
            var activator = ExpressionActivator.CreateActivator<Person>();

            var instance = activator.Invoke();

            instance.ShouldNotBeNull();
        }

        [Fact]
        public void CanCreateTypedInstanceWithParameters()
        {
            var activator = ExpressionActivator.CreateActivator<Person>(typeof(string), typeof(string));

            var instance = activator.Invoke("Kristian", "Hellang");

            instance.ShouldNotBeNull();
            instance.FirstName.ShouldEqual("Kristian");
            instance.LastName.ShouldEqual("Hellang");
        }

        private class Person
        {
            public Person()
            {
            }

            public Person(string firstName, string lastName)
            {
                FirstName = firstName;
                LastName = lastName;
            }

            public string FirstName { get; private set; }

            public string LastName { get; private set; }
        }
    }
}