---
layout: post
title:  "J’ai hacké mon onduleur ou le reverse engineering de protocoles de communication (part 8)"
date: 2006-09-01 15:02:00 +0100
---
J’ai donc continuer de développer ma solution de [gestion d’onduleur]({% post_url 2006-08-06-J’ai-hacké-mon-onduleur-ou-le-reverse-engineering-de-protocoles-de-communication-(part-1) %}). J’ai profité de mon retour à la maison pour continuer les tests physiques sur mes onduleurs. Oui, j’ai bien dit mes onduleurs. En fait, j’en possède 3 du même modèle. Ce qui a aussi motivé l’écriture de ce service de gestion… 

La suite des tests m’a permis de compléter [le protocole de communication]({% post_url 2006-08-15-J’ai-hacké-mon-onduleur-ou-le-reverse-engineering-de-protocoles-de-communication-(part-6) %}) et de déterminer à quoi servent la plupart des bits : 

00001000 = statut. 1 bit par info. 

b0b1b2b3b4b5b6b7 

  * b0 = passe à 1 lorsqu’une panne de courant arrive, lorsque l’alimentation est coupée       
  * b1 = passe à 1 lorsque la batterie est faible       
  * b2 = ??? pas encore déterminé. Impossible en utilisation normale de le faire passer à 1. Il reste à 0.       
  * b3 = ??? Idem       
  * b4 = est à 1 en fonctionnement normal. Il indique que les sorties sont alimentées       
  * b5= passe à 1 quand on est en mode test. Le mode test est déclenché par l’envoie de la commande T       
  * b6 = ??? pas non plus réussit à déterminer à quoi ce bit sert       
  * b7 = passe à 1 quand l’alimentation est sur batterie   
    Cela m’a également permis d’étalonner les niveau de la batterie. Le niveau haut varie de 14 à 10,7. L’échelle de consommation est quasi linéaire. 

Côté code, j’ai décidé d’implémenter des événements. Ainsi, lorsqu’un de ces bits change, j’envoie l’événement correspondant. Cela permet de gérer avec de la souplesse ce qui peut arriver. Il faut d’abord dans le code déclarer les événements : 

```vb
'se déclenche lorsque le secteur est coupé, en général va avec l'événement FonctionnementBatterie 
Public Event ArretAlimSecteur() 

'se déclenche lorsque le secteur est remis (et qu'il était coupé) 
Public Event RetourAlimSecteur() 

'lorsque l'onduleur fonctionne sur batterie 
Public Event FonctionnementBatterie() 

'ou lorsqu'il fonctionne de nouveau normalement 
Public Event ArretFonctionnementBatterie() 

'Lorsque la batterie est faible 
Public Event BatterieFaible() 

'lorsque l'onduleur est arrêté 
Public Event OnduleurArrete() 
```

Puis, pour chaque bit, on vérifie si un changement d’état s’est passé. Ca donne cela : 

```vb
'position 0 = panne secteur 
Dim BoolTemp As Boolean 
' récupère la valeur (0 ou 1) du bit 0 de la chaîne de 8 bits 
BoolTemp = Val(mc.Item(7).Value.Substring(0, 1)) 
If BoolTemp <> myOnduleurPanneSecteur Then 
  If BoolTemp Then 
    RaiseEvent ArretAlimSecteur() 
  Else 
    RaiseEvent RetourAlimSecteur() 
  End If 
myOnduleurPanneSecteur = BoolTemp 
End If 
```

Le principe est simple, il faut récupérer la valeur dans une variable temporaire. Puis on compare à la valeur actuelle. Si la valeur est la même, alors pas de changement d’état et on ne fait rien. Si les valeurs sont différentes, alors on regarde la valeur de la variable. 

Dans le cas où le bit est à 1, alors cela signifie que le courant a été coupé. On envoie donc l’événement ArretAlimSecteur. Dans le cas où il passe à 0, on peut signifier que le courant est revenu en envoyant l’événement RetourAlimSecteur. Le mot magic RaiseEvent permet d’envoyer l’événement. 

Ne pas oublier à la fin d’attribuer la nouvelle valeur à la variable privée stockant cette information. 

Côté du code qui intercepte les événements, ce n’est pas plus compliqué. Il faut déclarer une variable avec le mot clé WithEvents. 

```vb
Public WithEvents myOnduleur As New Onduleur 
```

On se retrouve ensuite avec une fonction de ce type qui permet d’intercepter l’événement et de le traiter : 

```vb
Private Sub myOnduleur_ArretAlimSecteur() Handles myOnduleur.ArretAlimSecteur 
'Faire quelque chose ici :-) 
End Sub 
```


S’il y a des paramètres à passer, cela se passe de la même façon. Il suffit de les décrire dans la définition puis de les envoyer ans le RaiseEvent et de les récupérer comme avec n’importe quel événement dans la fonction Handles. 

L’utilisation des événements est donc très simple. Un vrai bonheur. 

Avec cela, j’ai fini d’écrire ma classe de gestion de l’onduleur. Elle permet de se connecter à l’onduleur, de récupérer les informations, en cas de panne de courant ou autre, d’envoyer l’événement correspondant. 

Je récupère les données de façon régulière à l’aide d’un timer. Le fonctionnement d’un timer au prochain post… 



