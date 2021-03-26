---
layout: post
title:  "Monorepo for Beginners"
date: 2021-03-26 09:48:00 +0100
categories: 
author: "Laurent Ellerbach"
thumbnails: "/assets/2021-03-26-thumb.jpg"
---
# Monorepo for Beginners

*Monorepos store everything in a single repository, whether it’s code, documentation, or any other asset that has to do with the system you are building. This leads to a large number of commits and stakeholders, so how can you mitigate plunging it into chaos?*

With the digital transformation picking up steam, large and small manufactures find themselves hiring software teams and producing code. Unlike widgets however, code is infinitely flexible so everything can (and likely will) be changing over time. In this blog post, we want to share a Microsoft manufacturing customer’s journey that started off by placing a new heart in the company’s software system: the monorepository.

## Taming chaos

Many industrial processes aim to avoid losing control of what goes where, when, and by whom. Not only will that reduce risk (liability, injuries, …) but processes also keep everything managable despite evolving workforce, requirements, laws, etc. - or in other words: taming chaos. Software processes want to provide the same qualities:

- Requirements definition
- Requirements validation
- Requirements tracing from definition to test
- Requirement verification (aka tests)
- Maintainability
- Current status

Since people are at their best when they want to participate, providing them with an easy way to perform those tasks is key - and this is what the monorepository aims to do.

## Mono vs multi

A monolithic repository - or monorepo for short - is a way of storing everything related to a project in a single repository. While Git repositories are familiar to every software engineer, many like the clean one-repo-per-project approach. Those projects typically focus on a single application, spreading any related material (e.g. documentation) across several other repositories or services within the same organization. This is called a multiple repository (multirepo for short) approach.

Large undertakings, such as creating a platform for manufacturing analytics, can follow a multirepo approach but will soon realize the amount of upkeep required for with policies and knowledge sharing. On top of that, writing code is somewhat opinionated about things like operating system, editor, and how much whitespace is too much. Now imagine keeping a reasonably uniform standard across 100 repositories!

> Multirepos downsides:
>
> - No one view of the complete system
> - Changes require writing to multiple repositories
> - Tedious stakeholder management

The monorepo excels in forcing everybody to collaborate on the same terms. While this isn’t without issues, it allows participants to see the big picture, find documentation, or participate in discussions outside of their teams. On top of that, it’s much easier to enforce the same standards for each project - be it pipelines, editor configs, or whitespace; all of which lead to increased familiarity and a shared sense of responsibility.

> Monorepo upsides:
>
> - A complete view of the entire system
> - One repository is easy to watch and manage
> - Uniform standards across projects

How can those upsides be used?

## All glory to the monorepo?

Obviously this approach is not without downsides:

> Monorepo downsides:
>
> - Grows very large (in size and complexity)
> - Discipline and processes are required
> - Tedious access management

However these can be tackled with everybody’s favorite things: processes. In a monorepo, it’s not a single team determinining how code is written, documented, or stored - it’s all teams agreeing on a common standard. This standard (e.g. which libraries to use) is then enforced and controlled by two large measures: automation and peer reviews.

Automation provides a consistent, predictable ruleset that is applied wherever necessary to approve code contributions, build deployable packages, and run tests. On a repository service such as Azure DevOps or Github, that means creating [pipelines](https://azure.microsoft.com/en-us/services/devops/pipelines/)/[actions](https://github.com/features/actions) and bots to consistently produce high quality contributions. Wherever doable, automate processes so developers can focus on solving business problems without worrying about doing it wrong.

> Azure DevOps and Github provide ways to prohibit unapproved code from merging, so making pipeline runs mandatory is essential to keep the quality high. Branching policies protect mainline branches from direct edits.

Peer reviews are not only an integral part of the fork-merge workflow, but also provide opportunity to learn and improve for all participants. Most current git platforms provide a “merge/pull request” interface that allows looking at and commenting on the included changes. However this requires discipline and time, so managers have to allow for each team member to participate.

> As a developer, pull/merge requests are a great way to make sure you didn’t miss anything and to learn about alternative solutions to yours. Embrace the feedback!

In general, a monorepo is a place for large-scale collaboration, which means that it’s important to be disciplined and uphold processes (and adjust them) so others can do the same. Governance of these processes has to be delegated largely to the deveopers since they should want to follow and enforce them, since it’s the place the come to work to every day - and would you enjoy coming to a messy workplace each day?

## A closer look at the monorepo in practice

A few months ago we started implementing a monorepo strategy at a customer engagement. The transition from more than 10 individual repositories to a single one took several weeks to fully complete since everyone was now responsible for upholding the processes. For this to succeed each participant had to be clear on the goal, the procedures, and how to change them.

In order to prevent a chaotic monorepo, the team produced guidelines, documentation, and a few demos on what goes where - but most importantly the design tried to make it really obvious by keeping the top level directories simple:

```text
/
├── .pipelines               <- devops and automation files
├── data                     <- example data, bootstrapping data, etc.
├── docs                     <- documentation like a wiki or similar
│   └── platform             <- documentation specific for a team/workstream
├── infrastructure           <- infrastructure as code
│   ├── .pipelines           <- infrastructure pipeline files
│   ├── kubernetes
│   └── terraform
├── lib                      <- globally shared software components
│   └── MyGlobalCSharpLib
├── services                 <- micro services source code
│   ├── .pipelines           <- micro service build pipelines 
│   ├── MyCSharpService      <- a micro service project
│   ├── lib                  <- libraries for micro services only
│   │   └── my-local-js-lib
│   └── my-node-service
└── tests                    <- e.g. integration tests, no unit tests
```

By keeping the topmost folders clearly named and simple, without any special naming (e.g. teams wanted “per workstream names”) the folders should remain meaningful even if the project matured over years and thousands of developer hours. At the same time any developer is empowered to pick the right place for their code. To make sure to get the most of a monorepo, follow these four principles:

- **Scopes**: The folders act as scopes to make sure code artifacts are only visible when they should be. This allows to extract common tasks (e.g building a C# solution) quickly and maintainers can easier reason about where the error lies.
- **One ancestor**: Version control (especially Git) builds a hierarchical representation of the code and its changes. Therefore specialized versions (e.g. a custom fix for a unique problem) can be maintained much easier as a change sets are compatible.
- **Big pictures**: With everything in one place there is no need to copy code between repositories or to look for infrastructure as code files and documentation.
- **Good practice**: A monorepo requires teams to work with each other. By merging code only with a [pull request (PRs)](https://docs.microsoft.com/en-us/azure/devops/repos/git/pull-requests), teams review each other’s code which breaks silos and improves code quality.
With a basic folder structure available, let’s go over how to work with it.

## Code, merge, release, repeat

In terms of meetings, everybody knows [Scrum’s](https://www.atlassian.com/agile/scrum) ritualistic ways: standups, reviews, plannings, retrospecitves, etc. If done well, each sprint should yield a deployable service. So how can a monorepo support that? As a basic setup, two branches are required:

1. **main** This branch shows what currently is running in production
2. **dev** The latest and greatest features currently deployed in a testing environment
None of these two branches can be accessible directly, they can only be updated via pull requests. A quick overview can be found in the following image, with branch names in blue and annotations in green:

![repo strategy](/assets/Kj3IJPa.png)

Releasing a new version then becomes a matter of merging dev with main, with automation doing the roll-out. Consequently the production environment can be sealed off so nothing unexpected can be deployed there.

> In Azure DevOps, use branching policy to prohibit direct access to these branches. Set up DevOps pipelines to create immutable, repeatable, and tested deployments and identify them by the latest commit hash or [(Git) tag](https://git-scm.com/book/en/v2/Git-Basics-Tagging).

Clearly, computers are much better at following check lists and running through tasks, which is why a monorepo needs automation.

## Automation

Automating tasks is a core requirement for monorepos. Everybody makes mistakes and those mistakes are costly to clean up, so minimizing the chances for errors makes everybody happier and better off.

Many repositories services allow using pipelines (or something similar) to automate steps like build, test, checking for style or best practices, and anything that can be done with an API. This has two major upsides:

- The process can be tested and verified
- No interaction required

Monorepos have to use these pipelines to do the following:

- Run build and test ([CI](https://docs.microsoft.com/en-us/azure/devops/learn/what-is-continuous-integration)) before enabling a merge into the dev/main branches
- One-click deployments of the entire system from scratch (ideally 😅)

Additionally, many things can be automated - but it’s important to be able to trust the oucome as a developer. The trade-off is that sophisticated pipelines make “quick and dirty” local builds much more difficult. Consequently the teams have to be able to change the processes as needed.

> Automated interdependencies make it almost impossible to quickly fix a problem or try out something within the codebase.

## File and repository sizes

Since the repository will grow very large, a full clone takes a long time, which is inefficient if you only need a small part. Similarly, Git is not very good with binary files (images, PowerBI reports, …) and those will significantly grow the repository. In order to prevent bloating the monorepo, there are two solutions that most repository services support:

Git’s large file system (LFS) which diverts files by extension to a blob store, replacing it by a reference in the repo
Git’s virtual file system (VFS) provides a per-folder clone
For more detail on how these additions have been added for this particular project, check out the [post on Git LFS vs VFS for Git](https://mtirion.medium.com/working-with-large-git-repositories-using-git-lfs-or-vfs-for-git-db8ddccf7cbe).

## Wrapping it up

Software development is very opinionated on almost all matters, but especially how to work. A monorepo therefore poses a challenge to convince engineers to trust that other engineers are doing the right thing too and for that it needs proper governance. Once established however, these processes improve software quality and maintainability when required.

When does it pay off to convince everybody to adapt to new processes? We have found the following cases to improve significantly with a monorepo:

- The software consists of many linked but independent components
- Teams have end-to-end ownership of one or more such components
- The system is always deployed together

Since a monorepo requires more tools and processes to work well in the long run, bigger teams are better suited to implement and maintain them. Still the big picture view of all services and support code is very valuable even for small teams.

Here are some implementation examples with big codebases at [Microsoft](https://docs.microsoft.com/en-us/azure/devops/learn/devops-at-microsoft/use-git-microsoft), [Google](https://research.google/pubs/pub45424/), or [Facebook](https://engineering.fb.com/2014/01/07/core-data/scaling-mercurial-at-facebook/).

This [awesome monorepo list](https://github.com/korfuri/awesome-monorepo) provides a range of tools and solutions for common problems (scaling, builds, etc.) as well as some reading material.

This post was originally published on [blog.x5ff.xyz](https://blog.x5ff.xyz/).
