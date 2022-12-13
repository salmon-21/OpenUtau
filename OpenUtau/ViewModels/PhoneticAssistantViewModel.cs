﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using OpenUtau.Core.G2p;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace OpenUtau.App.ViewModels {
    public class PhoneticAssistantViewModel : ViewModelBase {
        public class G2pOption {
            public string name;
            public Type klass;
            public G2pOption(Type klass) {
                name = klass.Name;
                this.klass = klass;
            }
            public override string ToString() => name;
        }
        public List<G2pOption> G2ps => g2ps;

        [Reactive] public G2pOption? G2p { get; set; }
        [Reactive] public string Grapheme { get; set; }
        [Reactive] public string Phonemes { get; set; }

        private readonly List<G2pOption> g2ps = new List<G2pOption>() {
            new G2pOption(typeof(ArpabetG2p)),
            new G2pOption(typeof(FrenchG2p)),
            new G2pOption(typeof(PortugueseG2p)),
            new G2pOption(typeof(RussianG2p)),
        };

        private Api.G2pPack? g2p;

        public PhoneticAssistantViewModel() {
            Grapheme = string.Empty;
            Phonemes = string.Empty;
            this.WhenAnyValue(x => x.G2p)
                .Subscribe(option => {
                    g2p = null;
                    if (option != null) {
                        g2p = Activator.CreateInstance(option.klass) as Api.G2pPack;
                        Refresh();
                    }
                });
            this.WhenAnyValue(x => x.Grapheme)
                .Subscribe(_ => Refresh());
        }

        private void Refresh() {
            if (g2p == null) {
                Phonemes = string.Empty;
                return;
            }
            string[] phonemes = g2p.Query(Grapheme);
            if (phonemes == null) {
                Phonemes = string.Empty;
                return;
            }
            Phonemes = string.Join(' ', phonemes);
        }
    }
}
