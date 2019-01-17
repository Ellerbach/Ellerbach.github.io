---
layout: post
title:  "J’ai hacké mon onduleur ou le reverse engineering de protocoles de communication (part 9)"
date: 2006-09-14 05:34:00 +0100
---
Comme indiqué dans mon [précédent post]({% post_url 2006-09-01-J’ai-hacké-mon-onduleur-ou-le-reverse-engineering-de-protocoles-de-communication-(part-8) %}), je vais expliquer le fonctionnement d'un timer. Très utilise lorsqu'on souhaite faire une action spécifique à intervalle régulier. Je vais également en profiter pour montrer comment créer une propriété en lecture et écriture. En plus, je vais expliquer comment lever des exceptions dans son propre code. 

Une bonne partie de tout cela se trouve condensé dans la fonction d'ouverture de l'onduleur. Voici donc le code avec les déclarations de variables et constantes nécessaires à la compréhension. 

```vb
Const ERREUR_IDENTITE = "Impossible d'obtenir l'identité de l'onduleur" 
Const ERREUR_ALIM_BATTERIE = "Impossible d'obtenir les informations de batterie" 
Const ERREUR_ALIM_SECTEUR = "Impossible d'obtenir les informations de secteur" 
Const PERIOD_APPEL = 3000 '2 secondes = 2000 milisecondes 
Private myPeriodeAppel As Integer = PERIOD_APPEL 
Private myPort As SerialPort 
Private myPortSerie As String = "" 'stocke le nom du port 
Private myTimer As Timer 

Public Function Ouvrir(ByVal StrPort As String) As Boolean 
    Try 
        'ouvre le port série avec le bonnes infos 
        myPortSerie = StrPort 
        myPort = New SerialPort(StrPort, 2400, Ports.Parity.None, 8, Ports.StopBits.One) 
        'le caractère de fin de ligne est chr(13), c'est à dire Entrée 
        myPort.NewLine = Chr(13) 
        'délais d'attente de 2 secondes 
        myPort.ReadTimeout = 2000 
        myPort.Open() 
        myPort.DtrEnable = True 
        'vérifie que l'on a bien un onduleur et rempli les bonnes fonctions 
        If Not (EnvoiCommande(OnduleurCommande.Identite)) Then 
            Throw New System.Exception(ERREUR_IDENTITE) 
        End If 
        If Not (EnvoiCommande(OnduleurCommande.Batterie)) Then 
            Throw New System.Exception(ERREUR_ALIM_BATTERIE) 
        End If 
        If Not (EnvoiCommande(OnduleurCommande.Tension)) Then 
            Throw New System.Exception(ERREUR_ALIM_SECTEUR) 
        End If 
        'toutes les propriétés ont été remplies 
        'intialise le timer, récupère les infos toutes les 5 secondes 
        myTimer = New Timer(AddressOf VerifierStatut) 
        myTimer.Change(0, myPeriodeAppel) 
        Return True 
    Catch ex As Exception 
        'si ça ne s'ouvre pas, répercute l'exception 
        Throw ex 
        Return False 
    End Try 
End Function 
```

La fonction Ouvrir prend un paramètre qui doit contenir une chaîne de type COM1, COM2 ou autre permettant d'ouvrir le port série. Le tout se trouve dans un try – catch qui va permet de récupérer les exceptions qui peuvent être levées. En cas de problème, sur le port série, il est important de ne pas laisser planter l'application et de le remonter à l'appelant. 

Sur le même principe, lorsque le port série est ouvert, la fonction EnvoiCommande(OnduleurCommande.Identite) renvoie True si les informations de type modèle, sous modèle, version sont récupérés. Si ce n'est pas le cas, elle renvoie False. Du coup, si un onduleur n'est pas présent, les informations ne seront pas récupérées et False sera renvoyé. Dans ce cas, nous sommes devant un problème et on lève une exception. Cela se fait simplement à l'aide le la ligne Throw New System.Exception("ce que l'on veut envoyer"). Simple et efficace. 

Quand au timer, simple également. Il suffit de déclarer une variable de type Timer, ici myTimer. Ensuite, il faut créer une fonction de callback (rappel). Elle permettra au lors du déclanchement du timer de venir se brancher et d'exécuter le code de la fonction. La fonction de call back s'appelle VerifierStatut. Sa définiton est ci-dessous. L'initialisation se fait donc en indiquant myTimer = New Timer(AdressOf VerifierStatut). AdressOf renvoie un pointeur de la fonction de callback. Il ne reste plus qu'à initialiser le timer : myTimer.Change(0, myPeriodeAppel). 0 indique qu'il doit se déclencher maintenant. Le myPeriodeAppel indique l'intervalle en milli secondes d'appel de la fonction de callback. 

```vb
Private Sub VerifierStatut(ByVal stateInfo As Object) 
    'pas de vérification particulière. En cas de problème, les exceptions sont trappées dans la fonction EnvoiCommande 
    EnvoiCommande(OnduleurCommande.Tension) 
End Sub 
```

Dans mon cas, par défaut, lors de l'ouverture de l'onduleur, l'appel se fait toutes les 3000 milli secondes soit toutes les 3 secondes. J'ai implémenté en plus dans ma gestion d'onduleur une propriété qui permet de modifier cette valeur. Je ne sais pas encore par avance tous les combiens de temps, il est nécessaire de l'appeler. Donc, je me garde la possibilité de modifier cette valeur. Cependant, pour des questions de performances, je ne souhaite pas que l'appel soit fait à moins d'une seconde. J'exprime donc ma propriété en secondes et choisi des byte. 

L'implémentation de la propriété donne cela : 

```vb
Public Property PeriodeAppel() As Byte 
    Get 
        Return (myPeriodeAppel / 1000) 
    End Get 

    Set(ByVal value As Byte) 
        If value > 0 Then 
            myPeriodeAppel = value * 1000 
            myTimer.Change(0, myPeriodeAppel) 
        End If 
    End Set
End Property 
```

La lecture (Get) est simple, je renvoie la période d'appel divisée par 1000 car en interne ma variable est en milli secondes. 

Pour l'écriture (Set), je vérifie que la valeur est bien supérieur à 0 (donc au moins égale à 1). Je la multiplie par 1000, et je change la période d'appel de mon timer. 

Avec VB.NET, il est donc très facile de lever des exceptions, facile également d'implémenter et gérer un timer, de mettre en place des propriétés, en lecture et en écriture. Avec un comportement particulier qui modifie l'application dans la partie lecture ou écriture. 

Le prochain post sera certainement consacré à la mise en place d'un service Windows. Seule chose qu'il me reste à faire par rapport à mon [ambition d'origine]({% post_url 2006-08-06-J’ai-hacké-mon-onduleur-ou-le-reverse-engineering-de-protocoles-de-communication-(part-1) %}) ! So stay tune…

