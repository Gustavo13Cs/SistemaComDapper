using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Estacionamento.Repositorios
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class IgnoreInDapperAttribute : Attribute
    {
        public IgnoreInDapperAttribute() { }
    }
}
