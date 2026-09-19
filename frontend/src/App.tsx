import { useEffect, useMemo, useState } from 'react'
import './App.css'

type GameRequirement = {
  id: string
  title: string
  description: string
}

type MaintenanceTicket = {
  id: string
  title: string
  requester: string
  priority: string
  description: string
  tags: string[]
}

type PimIssue = {
  id: string
  ticketId: string
  title: string
  description: string
  skillArea: string
  points: number
  complexity: string
}

type SkillRating = {
  skill: string
  score: number
  level: string
}

type Achievement = {
  name: string
  description: string
  unlocked: boolean
}

type GameStage = {
  title: string
  description: string
}

type GameEvaluation = {
  resolvedIssueIds: string[]
  openIssues: PimIssue[]
  resolvedIssues: PimIssue[]
  completionPercent: number
  currentStage: GameStage
  skillRatings: SkillRating[]
  achievements: Achievement[]
  summary: string
}

type GameSnapshot = {
  productName: string
  scenario: string
  requirements: GameRequirement[]
  inbox: MaintenanceTicket[]
  issues: PimIssue[]
  evaluation: GameEvaluation
}

const apiBaseUrl = import.meta.env.VITE_API_URL ?? ''

function App() {
  const [game, setGame] = useState<GameSnapshot | null>(null)
  const [selectedIssueIds, setSelectedIssueIds] = useState<string[]>([])
  const [evaluation, setEvaluation] = useState<GameEvaluation | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    const loadGame = async () => {
      try {
        const response = await fetch(`${apiBaseUrl}/api/game`)

        if (!response.ok) {
          throw new Error('Unable to load the PIM maintenance sprint.')
        }

        const snapshot: GameSnapshot = await response.json()
        setGame(snapshot)
        setEvaluation(snapshot.evaluation)
        setSelectedIssueIds(snapshot.evaluation.resolvedIssueIds)
      } catch (loadError) {
        setError(loadError instanceof Error ? loadError.message : 'Unknown error')
      }
    }

    loadGame()
  }, [])

  const issuesByTicket = useMemo(() => {
    return game?.issues.reduce<Record<string, PimIssue[]>>((lookup, issue) => {
      lookup[issue.ticketId] = [...(lookup[issue.ticketId] ?? []), issue]
      return lookup
    }, {})
  }, [game])

  const toggleIssue = async (issueId: string) => {
    const nextSelection = selectedIssueIds.includes(issueId)
      ? selectedIssueIds.filter((currentId) => currentId !== issueId)
      : [...selectedIssueIds, issueId]

    setSelectedIssueIds(nextSelection)

    try {
      const response = await fetch(`${apiBaseUrl}/api/game/evaluate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ resolvedIssueIds: nextSelection }),
      })

      if (!response.ok) {
        throw new Error('Unable to update candidate progress.')
      }

      const nextEvaluation: GameEvaluation = await response.json()
      setEvaluation(nextEvaluation)
      setError('')
    } catch (evaluationError) {
      setError(
        evaluationError instanceof Error
          ? evaluationError.message
          : 'Unknown evaluation error',
      )
    }
  }

  if (error && !game) {
    return (
      <main className="page">
        <section className="panel">
          <h1>PIM Quest</h1>
          <p>{error}</p>
        </section>
      </main>
    )
  }

  if (!game || !evaluation) {
    return (
      <main className="page">
        <section className="panel">
          <h1>Loading PIM Quest…</h1>
        </section>
      </main>
    )
  }

  return (
    <main className="page">
      <section className="hero panel">
        <div>
          <p className="eyebrow">{game.productName}</p>
          <h1>PIM maintenance challenge</h1>
          <p className="lede">{game.scenario}</p>
          <a className="quick-link" href="#achievements">
            View achievements
          </a>
        </div>
        <div className="stage-card">
          <span>Current stage</span>
          <strong>{evaluation.currentStage.title}</strong>
          <p>{evaluation.currentStage.description}</p>
          <div className="progress-bar" aria-hidden="true">
            <span style={{ width: `${evaluation.completionPercent}%` }} />
          </div>
          <small>{evaluation.completionPercent}% sprint completion</small>
        </div>
      </section>

      {error ? <p className="error-banner">{error}</p> : null}

      <section className="grid">
        <article className="panel">
          <h2>Requirements</h2>
          <ul className="stack-list">
            {game.requirements.map((requirement) => (
              <li key={requirement.id}>
                <strong>{requirement.title}</strong>
                <p>{requirement.description}</p>
              </li>
            ))}
          </ul>
        </article>

        <article className="panel">
          <div className="action-heading">
            <div>
              <p className="section-kicker">Your action</p>
              <h2>Choose issues to resolve</h2>
            </div>
            <span className="action-badge">Updates automatically</span>
          </div>
          <p className="action-help">
            Select the maintenance issues you would fix. Your progress, resolved bugs, and skill rating update immediately.
          </p>
          <ul className="ticket-list">
            {game.inbox.map((ticket) => (
              <li key={ticket.id}>
                <div className="ticket-header">
                  <strong>{ticket.title}</strong>
                  <span>{ticket.priority}</span>
                </div>
                <p>{ticket.description}</p>
                <small>Requester: {ticket.requester}</small>
                <div className="tag-row">
                  {ticket.tags.map((tag) => (
                    <span key={tag}>{tag}</span>
                  ))}
                </div>
                <ul className="issue-list">
                  {(issuesByTicket?.[ticket.id] ?? []).map((issue) => (
                    <li key={issue.id}>
                      <label>
                        <input
                          type="checkbox"
                          checked={selectedIssueIds.includes(issue.id)}
                          onChange={() => {
                            void toggleIssue(issue.id)
                          }}
                        />
                        <span>
                          <strong>{issue.title}</strong>
                          <small>
                            {issue.skillArea} · {issue.points} pts · {issue.complexity}
                          </small>
                          <p>{issue.description}</p>
                        </span>
                      </label>
                    </li>
                  ))}
                </ul>
              </li>
            ))}
          </ul>
        </article>
      </section>

      <section className="grid">
        <article className="panel">
          <h2>Bugs to resolve from the start</h2>
          <ul className="bug-list">
            {evaluation.openIssues.map((issue) => (
              <li key={issue.id} className="bug-card">
                <div className="ticket-header">
                  <strong>{issue.title}</strong>
                  <span>{issue.complexity}</span>
                </div>
                <small>
                  {issue.skillArea} · {issue.points} pts
                </small>
                <p>{issue.description}</p>
              </li>
            ))}
          </ul>
        </article>

        <article className="panel">
          <h2>Resolved bugs</h2>
          <ul className="bug-list">
            {evaluation.resolvedIssues.length === 0 ? (
              <li className="bug-card empty-state">
                <p>No bugs resolved yet.</p>
              </li>
            ) : (
              evaluation.resolvedIssues.map((issue) => (
                <li key={issue.id} className="bug-card resolved">
                  <div className="ticket-header">
                    <strong>{issue.title}</strong>
                    <span>{issue.complexity}</span>
                  </div>
                  <small>
                    {issue.skillArea} · {issue.points} pts
                  </small>
                  <p>{issue.description}</p>
                </li>
              ))
            )}
          </ul>
        </article>
      </section>

      <section className="grid">
        <article className="panel">
          <h2>Skill rating</h2>
          <ul className="skill-list">
            {evaluation.skillRatings.map((skill) => (
              <li key={skill.skill}>
                <div className="ticket-header">
                  <strong>{skill.skill}</strong>
                  <span>{skill.level}</span>
                </div>
                <div className="progress-bar" aria-hidden="true">
                  <span style={{ width: `${skill.score}%` }} />
                </div>
                <small>{skill.score}/100</small>
              </li>
            ))}
          </ul>
        </article>

        <article className="panel" id="achievements">
          <h2>Achievements</h2>
          <ul className="achievement-list">
            {evaluation.achievements.map((achievement) => (
              <li
                key={achievement.name}
                className={achievement.unlocked ? 'achievement unlocked' : 'achievement'}
              >
                <strong>{achievement.name}</strong>
                <p>{achievement.description}</p>
              </li>
            ))}
          </ul>
          <p className="summary">{evaluation.summary}</p>
        </article>
      </section>
    </main>
  )
}

export default App
