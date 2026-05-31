using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Story.Core.Models;

namespace Story.Core.Engine
{
    public class MotorPoveste
    {
        public Poveste PovesteCurenta { get; private set; }
        public string IdBlocCurent { get; private set; }
        public Dictionary<string, int> StareAtribute { get; private set; } = new Dictionary<string, int>();

        private Dictionary<string, BlocPoveste> _indexBlocuri = new Dictionary<string, BlocPoveste>();
        private Stack<string> _istoric = new Stack<string>();

        public bool PoateMergeInapoi => _istoric.Count > 0;

        public void IncarcaPovesteJson(string jsonText)
        {
            PovesteCurenta = JsonSerializer.Deserialize<Poveste>(jsonText);

            if (PovesteCurenta == null)
                throw new Exception("Fișierul JSON nu a putut fi citit.");

            _indexBlocuri.Clear();
            foreach (var bloc in PovesteCurenta.Blocks)
            {
                if (!string.IsNullOrEmpty(bloc.Id))
                    _indexBlocuri[bloc.Id] = bloc;
            }

            StareAtribute.Clear();
            foreach (var atribut in PovesteCurenta.Attributes)
            {
                StareAtribute[atribut.Key] = atribut.Initial;
            }

            _istoric.Clear();

            if (!string.IsNullOrEmpty(PovesteCurenta.StartBlock) && _indexBlocuri.ContainsKey(PovesteCurenta.StartBlock))
                IdBlocCurent = PovesteCurenta.StartBlock;
            else
                IdBlocCurent = PovesteCurenta.Blocks.FirstOrDefault()?.Id;
        }
