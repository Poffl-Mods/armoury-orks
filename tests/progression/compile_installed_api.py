"""Compile the progression controller against installed game DLLs without running the game.
Usage: python3 tests/progression/compile_installed_api.py MANAGED MONO_BLEEDING_EDGE
"""
from pathlib import Path
import subprocess
import sys
import tempfile

managed, mono = map(Path, sys.argv[1:3])
root = Path(__file__).resolve().parents[2]
# Short symlinks avoid the comma in the installed game's path confusing mcs.
with tempfile.TemporaryDirectory(prefix='orks-api-') as folder:
    folder = Path(folder)
    references = []
    for name in ['Code', 'RogueTrader.GameCore', 'UnityEngine.CoreModule', 'Owlcat.Runtime.Core',
                 'Kingmaker.PubSubSystem.Core.Interfaces', 'StateHasher',
                 'Kingmaker.UI.Models.Tooltip.Base', 'Kingmaker.Blueprints.Base',
                 'Kingmaker.Blueprints.JsonSystem.Hepers', 'Kingmaker.ElementsSystem.Interfaces',
                 'Utility.DotNetExtensions']:
        source = managed / (name + '.dll')
        target = folder / source.name
        target.symlink_to(source.resolve())
        references.append('-r:' + str(target))
    subprocess.run([str(mono / 'bin/mcs'), '-target:library',
        '-out:' + str(folder / 'ArmouryOrks.Runtime.dll'),
        '-r:' + str(mono / 'lib/mono/4.8-api/Facades/netstandard.dll'),
        *references,
        str(root / 'Assets/Modifications/ArmouryOrks/Scripts/ChoppaProgressionController.cs')], check=True)
print('Progression controller compiled against installed APIs; no game execution performed.')
