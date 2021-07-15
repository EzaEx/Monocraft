using System;
using System.Collections.Generic;
using System.Text;

namespace Monocraft
{
    public class NoiseMaker
    { 
        //different noise generators
        private OpenSimplex2F _tgen, _agen, _hgen;
        public NoiseMaker(int seed)
        { 
            //init noise gens with different seeds, based on world seed
            _tgen = new OpenSimplex2F(seed);
            _agen = new OpenSimplex2F(seed * 2);
            _hgen = new OpenSimplex2F(seed * 3);
        }

        public OpenSimplex2F Tgen { get => _tgen; set => _tgen = value; }
        public OpenSimplex2F Agen { get => _agen; set => _agen = value; }
        public OpenSimplex2F Hgen { get => _hgen; set => _hgen = value; }
    }
}
