Chain Choppa — optimiertes Modell

Original: 912.580 Dreiecke
Optimiert: 25.000 Dreiecke, 12.400 geometrische Vertices (Unity kann an UV-/Normalen-Kanten weitere Vertices erzeugen).
Reduktion: 97,26 %.

ChainChoppa_25k.fbx: reduziertes Modell mit UVs und Material.
ChainChoppa_25k.blend: bearbeitbare Szene mit Vorschau-Beleuchtung.
Texturen für Import: BaseColor, Metallic, Roughness und Normal_Baked.
Normal_Baked: 2048x2048, vom Original inklusive Oberflächennormalen auf das reduzierte Modell übertragen.
Normal_Source: unveränderte Normal-Textur des Meshy-Originals, nur als Referenz.
Original_Front / Optimized_Front / Optimized_Back: gerenderte Vergleichsansichten.

Das Original im Downloads-Ordner wurde nicht verändert. Ausrichtung, Maßstab und Ursprung des importierten Modells wurden beibehalten.
FBX erneut importiert: 25.000 Dreiecke, UVs und vorhandene Texturpfade geprüft.
Bei der späteren Integration müssen Griffposition, Größe in der Hand und Owlcat-Materialmasken angepasst und im Spiel geprüft werden. Die Vorschauen sind Blender-Renderings. Noch keine Einbindung in Spiel-Blueprints oder Animation der Kette.

Aktuelle Spielversion ab 0.5.7: ChainChoppa_60k.blend / ChainChoppa_60k.fbx, neu aus dem Original auf exakt 60.000 Dreiecke reduziert. Dazu gehört ChainChoppa_Normal_60k.png. Die 25k-Dateien bleiben als vorherige Variante erhalten.
Das Spiel-FBX in Assets/.../Art/ChainChoppa.fbx verwendet einen um ca. 6 cm Richtung Klinge versetzten Griffpunkt und 90 Grad Drehung der Schneidkante um die Längsachse. Diese Anpassung erzeugt scripts/prepare_chain_choppa.py aus dem 60k-Blender-Modell. Die Gesamtlänge bleibt 1,04 m.

Ab 0.5.8: weitere 180 Grad Drehung der Waffe um ihre Längsachse gegenüber 0.5.7; Griffposition, Größe und 60k-Geometrie bleiben erhalten.
