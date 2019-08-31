using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DesignBootcamp
{
    public class Container
    {

        private Dictionary<Type, Type> _typeRegistrations = new Dictionary<Type, Type>();

        public Container Register<TContract, TImpl>() where TImpl : TContract
        {
            _typeRegistrations[typeof(TContract)] = typeof(TImpl);
            return this;
        }

        public T Create<T>()
        {
            return (T)Create(typeof(T));
        }

        object Create(Type type)
        {
            /*
             * If T is concrete type, create instance
             * Else get mapped concrete type and create instance of that.
             */
            var isConcreteType = type.IsAbstract == false && type.IsInterface == false;
            if (isConcreteType == true)
                return CreateInstance(type);
            var mappedType = _typeRegistrations[type];
            return CreateInstance(mappedType);
        }

        private object CreateInstance(Type type)
        {
            // Get constructor and invoke it
            var constructor = type.GetConstructors().First();
            var parameters = constructor.GetParameters();
            var values = Array.ConvertAll(parameters, p => Create(p.ParameterType));
            return constructor.Invoke(values);
        }
    }

    
    public class Program
    {
        public static void Main(string[] args)
        {
            var container = new Container();
            container
                .Register<IA, A>()
                .Register<IB, B>()
                .Register<IC, C>();
            var obj = container.Create<IA>();
            Console.ReadKey(true);
        }
    }


    public interface IA
    {
    }

    public class A : IA
    {
        public A(IB b)
        {
            B = b;
        }

        public IB B { get; set; }
    }

    public class B : IB
    {
        public B(IC c)
        {
            C = c;
        }

        public IC C { get; set; }
    }

    public class C : IC
    {

    }

    public interface IB
    { }

    public interface IC { }

}
