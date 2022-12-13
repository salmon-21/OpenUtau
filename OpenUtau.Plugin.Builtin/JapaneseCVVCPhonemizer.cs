﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;
using OpenUtau.Core.Ustx;
using Serilog;

namespace OpenUtau.Plugin.Builtin {
    [Phonemizer("Japanese CVVC Phonemizer", "JA CVVC", "TUBS",language:"JA")]
    public class JapaneseCVVCPhonemizer : Phonemizer {
        static readonly string[] plainVowels = new string[] {"あ","い","う","え","お","を","ん","ン"};
        static readonly string[] nonVowels = new string[]{"息","吸","R","-","k","ky","g","gy",
                                                           "s","sh","z","j","t","ch","ty","ts",
                                                           "d","dy","n","ny","h","hy","f","b",
                                                           "by","p","py","m","my","y","r","4",
                                                           "ry","w","v","ng","l","・",
        };

        static readonly string[] vowels = new string[] {
            "a=ぁ,あ,か,が,さ,ざ,た,だ,な,は,ば,ぱ,ま,ゃ,や,ら,わ,ァ,ア,カ,ガ,サ,ザ,タ,ダ,ナ,ハ,バ,パ,マ,ャ,ヤ,ラ,ワ",
            "e=ぇ,え,け,げ,せ,ぜ,て,で,ね,へ,べ,ぺ,め,れ,ゑ,ェ,エ,ケ,ゲ,セ,ゼ,テ,デ,ネ,ヘ,ベ,ペ,メ,レ,ヱ",
            "i=ぃ,い,き,ぎ,し,じ,ち,ぢ,に,ひ,び,ぴ,み,り,ゐ,ィ,イ,キ,ギ,シ,ジ,チ,ヂ,ニ,ヒ,ビ,ピ,ミ,リ,ヰ",
            "o=ぉ,お,こ,ご,そ,ぞ,と,ど,の,ほ,ぼ,ぽ,も,ょ,よ,ろ,を,ォ,オ,コ,ゴ,ソ,ゾ,ト,ド,ノ,ホ,ボ,ポ,モ,ョ,ヨ,ロ,ヲ",
            "n=ん",
            "u=ぅ,う,く,ぐ,す,ず,つ,づ,ぬ,ふ,ぶ,ぷ,む,ゅ,ゆ,る,ゥ,ウ,ク,グ,ス,ズ,ツ,ヅ,ヌ,フ,ブ,プ,ム,ュ,ユ,ル,ヴ",
            "N=ン",
            "・=・",
        };

        static readonly string[] consonants = new string[] {
            "ch=ち,ちぇ,ちゃ,ちゅ,ちょ",
            "gy=ぎ,ぎぇ,ぎゃ,ぎゅ,ぎょ",
            "ts=つ,つぁ,つぃ,つぇ,つぉ",
            "ty=てぃ,てぇ,てゃ,てゅ,てょ",
            "py=ぴ,ぴぇ,ぴゃ,ぴゅ,ぴょ",
            "ry=り,りぇ,りゃ,りゅ,りょ",
            "ly=リ,リェ,リャ,リュ,リョ",
            "ny=に,にぇ,にゃ,にゅ,にょ",
            "r=ら,る,るぃ,れ,ろ",
            "hy=ひ,ひぇ,ひゃ,ひゅ,ひょ",
            "dy=でぃ,でぇ,でゃ,でゅ,でょ",
            "by=び,びぇ,びゃ,びゅ,びょ",
            "b=ば,ぶ,ぶぃ,べ,ぼ",
            "d=だ,で,ど,どぃ,どぅ",
            "g=が,ぐ,ぐぃ,げ,ご",
            "f=ふ,ふぁ,ふぃ,ふぇ,ふぉ",
            "h=は,はぃ,へ,ほ,ほぅ",
            "k=か,く,くぃ,け,こ",
            "j=じ,じぇ,じゃ,じゅ,じょ,ぢ,ぢぇ,ぢゃ,ぢゅ,ぢょ",
            "m=ま,む,むぃ,め,も",
            "n=な,ぬ,ぬぃ,ね,の",
            "p=ぱ,ぷ,ぷぃ,ぺ,ぽ",
            "s=さ,す,すぃ,せ,そ",
            "sh=し,しぇ,しゃ,しゅ,しょ",
            "t=た,て,と,とぃ,とぅ",
            "v=ヴ,ヴぁ,ヴぃ,ヴぅ,ヴぇ,ヴぉ",
            "ky=き,きぇ,きゃ,きゅ,きょ",
            "w=うぃ,うぅ,うぇ,うぉ,わ,ゐ,ゑ,を,ヰ,ヱ",
            "y=いぃ,いぇ,や,ゆ,よ",
            "z=ざ,ず,ずぃ,ぜ,ぞ",
            "dz=づ,づぃ",
            "my=み,みぇ,みゃ,みゅ,みょ",
            "ng=ガ,ギ,グ,ゲ,ゴ,ギェ,ギャ,ギュ,ギョ,カ゜,キ゜,ク゜,ケ゜,コ゜,キ゜ェ,キ゜ャ,キ゜ュ,キ゜ョ",
            "l=ラ,ル,レ,ロ",
            "・=・あ,・い,・う,・え,・お,・ん,・を,・ン",
        };

        // in case voicebank is missing certain symbols
        static readonly string[] substitution = new string[] {  
            "ty,ch,ts=t", "j,dy=d", "gy=g", "ky=k", "py=p", "ny=n", "ry=r", "hy,f=h", "by,v=b", "dz=z", "l=r", "ly=l"
        };

        static readonly Dictionary<string, string> vowelLookup;
        static readonly Dictionary<string, string> consonantLookup;
        static readonly Dictionary<string, string> substituteLookup;

        static JapaneseCVVCPhonemizer() {
            vowelLookup = vowels.ToList()
                .SelectMany(line => {
                    var parts = line.Split('=');
                    return parts[1].Split(',').Select(cv => (cv, parts[0]));
                })
                .ToDictionary(t => t.Item1, t => t.Item2);
            consonantLookup = consonants.ToList()
                .SelectMany(line => {
                    var parts = line.Split('=');
                    return parts[1].Split(',').Select(cv => (cv, parts[0]));
                })
                .ToDictionary(t => t.Item1, t => t.Item2);
            substituteLookup = substitution.ToList()
                .SelectMany(line => {
                    var parts = line.Split('=');
                    return parts[0].Split(',').Select(orig => (orig, parts[1]));
                })
                .ToDictionary(t => t.Item1, t => t.Item2);
        }

        // Store singer in field, will try reading presamp.ini later
        private USinger singer;
        public override void SetSinger(USinger singer) => this.singer = singer;

        // make it quicker to check multiple oto occurrences at once rather than spamming if else if
        private bool checkOtoUntilHit(string[] input, Note note, out UOto oto){
            oto = default;

            var attr0 = note.phonemeAttributes?.FirstOrDefault(attr => attr.index == 0) ?? default;
            var attr1 = note.phonemeAttributes?.FirstOrDefault(attr => attr.index == 1) ?? default;

            foreach (string test in input){
                if (singer.TryGetMappedOto(test, note.tone + attr0.toneShift, attr0.voiceColor, out oto)){
                    return true;
                }
            }

            return false;
        }

        // can probably be cleaned up more but i have work in the morning. have fun.
        public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevNeighbours) {
            var note = notes[0];
            var currentUnicode = ToUnicodeElements(note.lyric);
            var currentLyric = note.lyric;
            var cfLyric = $"* {currentLyric}";
            var attr0 = note.phonemeAttributes?.FirstOrDefault(attr => attr.index == 0) ?? default;
            var attr1 = note.phonemeAttributes?.FirstOrDefault(attr => attr.index == 1) ?? default;

            if (prevNeighbour == null) {
                // Use "- V" or "- CV" if present in voicebank
                var initial = $"- {currentLyric}";
                string[] tests = new string[] {initial, currentLyric};
                // try [- XX] before trying plain lyric
                if (checkOtoUntilHit(tests, note, out var oto)){
                    currentLyric = oto.Alias;
                }
            } else if (plainVowels.Contains(currentLyric) || nonVowels.Contains(currentLyric)) {
                var prevUnicode = ToUnicodeElements(prevNeighbour?.lyric);
                // Current note is VV
                if (vowelLookup.TryGetValue(prevUnicode.LastOrDefault() ?? string.Empty, out var vow)) {
                    var vowLyric = $"{vow} {currentLyric}";
                    // try vowlyric before cflyric, if both fail try currentlyric
                    string[] tests = new string[] {vowLyric, cfLyric, currentLyric};
                    if (checkOtoUntilHit(tests, note, out var oto)){
                        currentLyric = oto.Alias;
                    }
                }
            } else {
                string[] tests = new string[] {currentLyric};
                if (checkOtoUntilHit(tests, note, out var oto)){
                    currentLyric = oto.Alias;
                }
            }

            if (nextNeighbour != null) {

                var nextUnicode = ToUnicodeElements(nextNeighbour?.lyric);
                var nextLyric = string.Join("", nextUnicode);

                // Check if next note is a vowel and does not require VC
                if (nextUnicode.Count < 2 && plainVowels.Contains(nextUnicode.FirstOrDefault() ?? string.Empty)) {
                    return new Result {
                        phonemes = new Phoneme[] {
                            new Phoneme() {
                                phoneme = currentLyric,
                            }
                        },
                    };
                }

                // Insert VC before next neighbor
                // Get vowel from current note
                var vowel = "";
                if (vowelLookup.TryGetValue(currentUnicode.LastOrDefault() ?? string.Empty, out var vow)) {
                    vowel = vow;
                }

                // Get consonant from next note
                var consonant = "";
                if (consonantLookup.TryGetValue(nextUnicode.FirstOrDefault() ?? string.Empty, out var con)
                    || nextUnicode.Count >= 2 && consonantLookup.TryGetValue(string.Join("", nextUnicode.Take(2)), out con)) {
                    consonant = con;
                }


                if (consonant == "") {
                    return new Result {
                        phonemes = new Phoneme[] {
                            new Phoneme() {
                                phoneme = currentLyric,
                            }
                        },
                    };
                }

                var vcPhoneme = $"{vowel} {consonant}";
                var vcPhonemes = new string[] {vcPhoneme, ""};
                // find potential substitute symbol
                if (substituteLookup.TryGetValue(consonant ?? string.Empty, out con)){
                        vcPhonemes[1] = $"{vowel} {con}";
                }
                //if (singer.TryGetMappedOto(vcPhoneme, note.tone + attr0.toneShift, attr0.voiceColor, out var oto1)) {
                if (checkOtoUntilHit(vcPhonemes, note, out var oto1)) {
                    vcPhoneme = oto1.Alias;
                } else {
                    return new Result {
                        phonemes = new Phoneme[] {
                            new Phoneme() {
                                phoneme = currentLyric,
                            }
                        },
                    };
                }

                int totalDuration = notes.Sum(n => n.duration);
                int vcLength = 120;
                var nextAttr = nextNeighbour.Value.phonemeAttributes?.FirstOrDefault(attr => attr.index == 0) ?? default;
                if (singer.TryGetMappedOto(nextLyric, nextNeighbour.Value.tone + nextAttr.toneShift, nextAttr.voiceColor, out var oto)) {
                    vcLength = MsToTick(oto.Preutter);
                }
                vcLength = Math.Min(totalDuration / 2, vcLength);

                return new Result {
                    phonemes = new Phoneme[] {
                        new Phoneme() {
                            phoneme = currentLyric,
                        },
                        new Phoneme() {
                            phoneme = vcPhoneme,
                            position = totalDuration - vcLength,
                        }
                    },
                };
            }

            // No next neighbor
            return new Result {
                phonemes = new Phoneme[] {
                    new Phoneme {
                        phoneme = currentLyric,
                    }
                },
            };
        }
    }
}
