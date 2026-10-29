using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace yuyiAvalonia.ImageEdit.Models
{
    public class ImageIntensityCollection : ICollection<ImageIntensity>
    {
        private readonly List<ImageIntensity> _images;

        public ImageIntensityCollection()
        {
            _images = new List<ImageIntensity>();
        }

        public int Count => _images.Count();

        public bool IsReadOnly => false;

        public void Add(ImageIntensity item)
        {
            _images.Add(item);
        }

        public void AddRange(ImageIntensity[] items)
        {
            _images?.AddRange(items);
        }

        public void Clear()
        {
            _images.Clear();
        }

        public bool Contains(ImageIntensity item)
        {
            return _images.Contains(item);
        }

        public void CopyTo(ImageIntensity[] array, int arrayIndex)
        {
            _images.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ImageIntensity> GetEnumerator()
        {
            return _images.GetEnumerator();
        }

        public bool Remove(ImageIntensity item)
        {
            return _images.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _images.GetEnumerator();
        }
    }
}
