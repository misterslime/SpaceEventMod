using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEventMod;

[Serializable]
internal class InvalidTypeParameterException : Exception
{
    public InvalidTypeParameterException() : base()
    {
    }

    public InvalidTypeParameterException(string message) : base(message)
    {
    }

    public InvalidTypeParameterException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
