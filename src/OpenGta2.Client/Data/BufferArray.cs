using System;

namespace OpenGta2.Client.Data;

public class BufferArray<T>
{
    private T[] _buffer = new T[16];

    private int _length;

    public int Length => _length;

    public void Reset(bool clear = false)
    {
        _length = 0;
        if (clear)
            Array.Clear(_buffer);
    }

    private void Resize()
    {
        var buffer = new T[_buffer.Length * 2];
        Array.Copy(_buffer, 0, buffer, 0, _length);
        _buffer = buffer;
    }

    public void Add(T value)
    {
        if (_buffer.Length == _length)
            Resize();

        _buffer[_length++] = value;
    }

    public T[] GetArray()
    {
        return _buffer;
    }

    public Span<T> AsSpan()
    {
        return _buffer.AsSpan(0, _length);
    }
}