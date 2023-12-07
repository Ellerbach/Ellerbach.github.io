# 2016-08-17 J'ai hacké mon onduleur ou le reverse engineering de protocoles de communication (part 7)

Me voici donc maintenant avec 3 chaînes de texte à analyser et dont il faut que je ressorte plusieurs informations. Les chaînes sont bien délimitées. Le protocole complet est expliqué dans le [sixième post](./2006-08-15-J'ai-hacké-mon-onduleur-ou-le-reverse-engineering-de-protocoles-de-communication-(part-6).md).

Voici les 3 principales chaînes :

* #BELKIN Master 1.00
* (238.0 237.0 236.0 024 50.0 13.9 32.0 00001000
* #230.0 2.2 12.00 50.0
    En temps normal, je rechercherais les espaces, j'extrairai les valeurs entre les espaces et les convertirais. Vu que les chaînes semblent avoir tout le temps la même taille, même pas la peine de regarder les espaces, un extrait de type mid( "#230.0 2.2 12.00 50.0",1,5) pour le 230.0 suffirait amplement. Chaque partie peut être extraite ainsi. Une simple vérification de longueur de chaîne suffira. Histoire de compliquer un peu la chose et surtout dans la philosophie de me remettre au code et d'apprendre de nouvelles choses, je me suis dit que je regarderais bien les expressions régulières. Et là, le bonheur. Je connaissais un peu la théorie mais pas du tout la pratique. Et je me suis bien amusé :-).

Comme toujours, j'ai commencé par rechercher de l'info sur le sujet. D'abord la doc officielle sur le sujet (bof), puis des exemples sur MSDN (mieux) et dans les communautés (mieux). Je suis tombé sur pas mal d'applications qui permettaient de valider et tester ces expressions régulières et même de générer le code associé. Bien pratique pour se mettre en jambe.

J'ai donc suivi quelques tutoriaux, fait quelques exemples et je me suis lancé pour mes propres chaînes. Je suis parti du principe, pour toutes les chaînes, que la taille des chaînes ne varierai pas et qu'elles étaient toujours formées de la même façon. Que si cela n'était pas respecté, il y avait erreur. Que pour la chaîne contenant du texte, il n'y avait que 2 mots renvoyés. Voici donc un peu de code (toujours pour faire plaisir à [Benjamin](https://www.benjamingauthey.com)) pour illustrer cela :

```vb
Public Function DecryptTest(ByVal StrCrypter As String, ByVal IntTypeCrypter As OnduleurCommande) As Boolean 

  Select Case IntTypeCrypter 

    Case OnduleurCommande.Identite 
      ' on décrypte le I : #BELKIN Master 1.00 
      Dim re As New Regex("\b[A-Za-z].*?\b|\b\d\.\d\d\b") 
      Dim mc As MatchCollection 
      mc = re.Matches(StrCrypter) 
      If mc.Count = 3 Then 
        myModele = mc.Item(0).Value 
        mySousModele = mc.Item(1).Value 
        myModeleVersion = mc.Item(2).Value 
      Else 
        Return False 
      End If 

    Case OnduleurCommande.AlimentationBatterie 
      ' on décrypte le F : #230.0 2.2 12.00 50.0 
      Dim re As New Regex("\b\d\d\d\.\d\b|\b\d\.\d\b|\b\d\d\.\d\d\b|\b\d\d\.\d") 
      Dim mc As MatchCollection 
      mc = re.Matches(StrCrypter) 
      If mc.Count = 4 Then 
        myTensionSortieBatterie = Val(mc.Item(0).Value) 
        myAmperageBatterie = Val(mc.Item(1).Value) 
        myTensionBatterie = Val(mc.Item(2).Value) 
        myFrequenceSortieBatterie = Val(mc.Item(3).Value) 
      Else 
        Return False 
      End If 

    Case OnduleurCommande.AlimentationSecteur 
      ' on décrypte le Q1 : (238.0 237.0 236.0 024 50.0 13.9 32.0 00001000 
      Dim re As New Regex("\b\d\d\d\.\d\b|\b\d\d\d\b|\b\d\d\.\d\b|\b\d\d\d\d\d\d\d\d") 
      Dim mc As MatchCollection 
      mc = re.Matches(StrCrypter) 
      If mc.Count = 8 Then 
        myTensionAlim = Val(mc.Item(0).Value) 
        myTensionSortiePrimaire = Val(mc.Item(1).Value) 
        myTensionSortieSecondaire = Val(mc.Item(2).Value) 
        myPourcentageChargeOnduleur = Val(mc.Item(3).Value) 
        myFrequence = Val(mc.Item(4).Value) 
        myPourcentageChargeBatterie = (Val(mc.Item(5).Value) / 14.0 * 100.0) 
        myTemperature = Val(mc.Item(6).Value) 
        ' on s'occupera plus tard de la chaîne de 8 caractères 
      Else 
        Return False 
      End If 

    Case Else 
      Return False 

  End Select 
  Return True 

End Function 
```

Pour lire le code, tous les myQuelqueChose sont des variables privées qui contiennent toutes les propriétés de l'onduleur. Le type dépend de la variable à stocker. La fonction Val a l'avantage d'être surchargée et de renvoyer des types standards.

Analysons l'extraction des informations de la chaîne Q1. Création d'une expression régulière :

```vb
Dim re As New Regex("\b\d\d\d\.\d\b|\b\d\d\d\b|\b\d\d\.\d\b|\b\d\d\d\d\d\d\d\d") 
```

Création d'une collection d'informations (type String) renvoyées par le passage à l'expression régulière :

```vb
Dim mc As MatchCollection 
```

Passage de l'expression régulière sur la chaîne de caractère StrCrypter qui contient (238.0 237.0 236.0 024 50.0 13.9 32.0 00001000 :

```vb
mc = re.Matches(StrCrypter) 
```

S'il y a eu 8 morceaux trouvés alors c'est OK et on décrypte la chaîne :

```vb
If mc.Count = 8 Then 
```

Comment fonctionne l'expression régulière ? Avant de répondre, je précise que je ne suis pas devenu un expert en quelques jours donc, il y a peut-être mieux que ce que j'ai fait, plus simple ou plus compliqué. Mais en tout cas, cela fonctionne très bien :

* \b\d\d\d\.\d\b :

  * \d signifie valeur numérique, c'est la même chose que lorsqu'on écrit [0-9], je ne me prive donc pas d'utiliser le raccourci :-)
  * \. Désigne le point en tant que tel. Si . est utilisé seul, il désigne n'importe quel caractère.
  * \b indique qu'il faut qu'il y ait un espace ou une ponctuation avant le caractère
  * Cette expression me permet d'extraire les 3 premières chaînes de type 238.0

* |\b\d\d\d\b :

  * | signifie ou logique. Ce qui suit sera donc extrait en plus de ce qui précède
  * 3 \d indique 3 chiffres associé au \b du début, il permet d'extraire le 024

* |\b\d\d\.\d\b :

  * Rien de nouveau dans cette expression, même logique que la première. Elle permet d'extraire les 50.0, 13.9 et 32.0

* |\b\d\d\d\d\d\d\d\d :

  * Tout simple, récupère les 8 derniers chiffres
      La collection dans les recherches est toujours remplie dans l'ordre où les éléments ont été trouvés. Il ne me reste donc plus qu'après à remplir mes variables privées avec les informations trouvées, dans l'ordre où elles apparaissent.

Côté du texte, j'ai un peu rusé et utilisé une combine. L'expression \b[A-Za-z].*?\b me permet d'extraire une chaîne de texte située entre 2 espaces ou ponctuation quelque soit la longueur de la chaîne. Tout réside dans le qualificateur paresseux*? qui permet de renvoyer aussi peu de répétition que possible. Associé au \b qui le suit et au . qui le précède, il renvoie donc ce qui se trouve avant les ponctuations. Et permet de découper les mots. Je sens que je vais le garder dans mes favoris :-)

J'aurais également pu valider que la chaîne comprenait bien BELKIN, Master et 1.00 en utilisant l'expression régulière BELKIN|Master|1\.00 qui doit me renvoyer 3 chaînes. J'ai préféré l'autre solution car elle me paraissait un peu plus souple. Et surtout parce que j'étais tout fier d'avoir trouvé une expression sympa pour séparer les mots ! Cela rend mon code un peu plus générique et permettra une utilisation avec un autre type d'onduleur utilisant le même protocole.

Me voilà donc maintenant bien avancé avec mon protocole presque complet, le code qui permet de le décrypter, mon code d'envoi de mail. Il me reste donc à coder un peu d'intelligence pour permettre la remontée d'alerte, l'utilisation en tant que service Windows (hors de question de devoir être logué pour que ce programme fonctionne) et coder la communication avec l'onduleur. Bref, stay tune, la suite dans les prochains posts.
